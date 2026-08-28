using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.Database.RecipesDomain;
using Reciplex.Server.Database.Strategies;
using Reciplex.Server.Meilisearch;
using Reciplex.Server.Meilisearch.Responses;

namespace Reciplex.Server.Database.SearchExporter;

internal sealed class SearchExporterService(
    IServiceProvider sp,
    ILogger<SearchExporterService> logger,
    RepeatedDatabaseActionStrategy repeatedDatabaseActionStrategy,
    IClock clock
) : BackgroundService
{
    /// <summary>
    /// Recipe search index
    /// </summary>
    private const string RecipesSearchIndex = "recipes";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool anyWorkDone = false;
        anyWorkDone |= await repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            DeletionDatabaseActionStrategy.CollectAndDelete(
                (db, ct) =>
                    db
                        .RecipeSearchExtractionStatusEntries.Where(e =>
                            (
                                e.Recipe!.Deleted != null
                                || e.RecipeBook!.Deleted != null
                                || e.RecipeBook!.Owner!.Deleted != null
                            )
                            && e.SearchVersion == null
                            && e.TaskUid == null
                        )
                        .Select(e => e.Id)
                        .Take(100)
                        .ToListAsync(ct),
                (db, ids, ct) =>
                    db
                        .RecipeSearchExtractionStatusEntries.Where(e =>
                            ids.Contains(e.Id) && e.SearchVersion == null && e.TaskUid == null
                        )
                        .ExecuteDeleteAsync(ct),
                (
                    collectTime,
                    deleteTime,
                    rowCount
                ) => { /* todo */
                }
            ),
            (
                success,
                time
            ) => { /* todo */
            },
            (
                ex
            ) => { /* todo */
            },
            stoppingToken
        );

        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates an index with name <paramref name="indexName"/> and <paramref name="primaryKey"/>
    /// if it does not already exist.
    /// </summary>
    /// <param name="searchClient">the http client for the remote search server</param>
    /// <param name="indexName">the name of the index</param>
    /// <param name="primaryKey">the primary key of the index</param>
    /// <param name="ct">the async cancellation token</param>
    private static async Task UpsertIndexAsync(
        IMeilisearchClient searchClient,
        string indexName,
        string primaryKey,
        CancellationToken ct
    )
    {
        GetIndexResponse? index = await searchClient.GetIndexAsync(indexName, ct);
        if (index is null)
        {
            CreateIndexResponse createResponse = await searchClient.CreateIndexAsync(
                indexName,
                primaryKey,
                ct
            );
            TaskStatusResponse result = await searchClient.WaitForTaskCompletionAsync(
                createResponse.TaskUid,
                ct
            );
            result.EnsureSuccess();
        }
    }

    /// <summary>
    /// Opens a scope and exports changes to Meilisearch
    /// </summary>
    /// <param name="ct">async cancellation token</param>
    /// <returns></returns>
    private async Task ExportChangedRecordsAsync(CancellationToken ct)
    {
        await using AsyncServiceScope scope = sp.CreateAsyncScope();
        await using ApplicationDbContext db =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IMeilisearchClient searchClient =
            scope.ServiceProvider.GetRequiredService<IMeilisearchClient>();

        List<RecipeDatabaseExtractionRow> entries = await db
            .RecipeSearchExtractionStatusEntries.AsNoTracking()
            .Where(e =>
                (e.SearchVersion == null || e.Recipe!.SearchVersion != e.SearchVersion)
                && e.Recipe!.Deleted == null
                && e.RecipeBook!.Deleted == null
                && e.RecipeBook!.Owner!.Deleted == null
                && (e.ErrorCount < 3 || e.ErrorSearchVersion != e.Recipe.SearchVersion)
            )
            .Select(e => new RecipeDatabaseExtractionRow(
                e.RecipeFk,
                e.Id,
                e.RecipeBookFk,
                e.Recipe!.Name,
                e.Recipe.ShortDescription,
                e.Recipe.SearchVersion,
                e.ErrorCount,
                e.ErrorSearchVersion
            ))
            .Take(20)
            .ToListAsync(ct);

        if (entries.Count < 1)
        {
            return;
        }

        try
        {
            await ExportRecordBatchAsync(db, searchClient, entries, ct);
            return;
        }
        catch (Exception)
        {
            // todo - log
        }

        foreach (RecipeDatabaseExtractionRow entry in entries)
        {
            try
            {
                await ExportRecordBatchAsync(db, searchClient, [entry], ct);
            }
            catch (Exception)
            {
                // todo - log
                try
                {
                    await db
                        .RecipeSearchExtractionStatusEntries.Where(e => e.Id == entry.SearchEntryId)
                        .ExecuteUpdateAsync(
                            setter =>
                                setter
                                    .SetProperty(
                                        e => e.ErrorCount,
                                        e =>
                                            e.ErrorSearchVersion == entry.SearchVersion
                                                ? e.ErrorCount + 1
                                                : 1
                                    )
                                    .SetProperty(e => e.ErrorSearchVersion, entry.SearchVersion),
                            ct
                        );
                }
                catch (Exception)
                {
                    // todo - log
                }
            }
        }
    }

    private async Task ExportRecordBatchAsync(
        ApplicationDbContext db,
        IMeilisearchClient searchClient,
        List<RecipeDatabaseExtractionRow> entries,
        CancellationToken ct
    )
    {
        UpsertDocumentsResponse result = await searchClient.UpsertDocumentsAsync(
            RecipesSearchIndex,
            entries.Select(e => new RecipeSearchIndexEntry()
            {
                Id = MakeRecipeStringKey(e.RecipeId),
                Name = e.RecipeName,
                ShortDescription = e.RecipeDescription,
                BookId = MakeRecipeBookStringKey(e.RecipeBookFk),
            }),
            ct
        );

        List<long> searchIndexEntries = [.. entries.Select(e => e.SearchEntryId)];
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        await db
            .RecipeSearchExtractionStatusEntries.Where(e => searchIndexEntries.Contains(e.Id))
            .ExecuteUpdateAsync(
                setter =>
                    setter
                        .SetProperty(e => e.TaskUid, result.TaskUid)
                        .SetProperty(e => e.TaskPostTime, now),
                ct
            );

        TaskStatusResponse taskStatus = await searchClient.WaitForTaskCompletionAsync(
            result.TaskUid,
            ct
        );

        if (taskStatus.Status == MeilisearchTaskStatus.Succeeded)
        {
            foreach (var entry in entries)
            {
                await db
                    .RecipeSearchExtractionStatusEntries.Where(e =>
                        e.TaskUid == result.TaskUid && e.Id == entry.SearchEntryId
                    )
                    .ExecuteUpdateAsync(
                        setter =>
                            setter
                                .SetProperty(e => e.TaskUid, (long?)null)
                                .SetProperty(e => e.TaskPostTime, (long?)null)
                                .SetProperty(e => e.SearchVersion, entry.SearchVersion)
                                .SetProperty(e => e.ErrorCount, 0)
                                .SetProperty(e => e.ErrorSearchVersion, (long?)null),
                        ct
                    );
            }
        }
        else if (
            taskStatus.Status == MeilisearchTaskStatus.Failed
            || taskStatus.Status == MeilisearchTaskStatus.Canceled
        )
        {
            await db
                .RecipeSearchExtractionStatusEntries.Where(e => e.TaskUid == result.TaskUid)
                .ExecuteUpdateAsync(
                    setter =>
                        setter
                            .SetProperty(e => e.TaskUid, (long?)null)
                            .SetProperty(e => e.TaskPostTime, (long?)null),
                    ct
                );
            taskStatus.EnsureSuccess();
        }
        else
        {
            throw new NotImplementedException(
                $"task with id {taskStatus.Uid} has an unexpected status : {taskStatus.Status}"
            );
        }
    }

    /// <summary>
    /// Creates empty search entries for new records but does not extract them.
    /// </summary>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of new entries created</returns>
    /// <remarks>
    /// We use the <see cref="RecipeSearchExtractionStatusDbObject" />
    /// object to know if there could have been an extraction.
    /// If no entry exists for a record, then we know it
    /// was never extracted. If a record happens to exist,
    /// then we know we need to check first before deleting.
    /// </remarks>
    private async Task<long> PopulateEmptySearchEntries(CancellationToken ct)
    {
        await using AsyncServiceScope scope = sp.CreateAsyncScope();
        await using ApplicationDbContext db =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IMeilisearchClient searchClient =
            scope.ServiceProvider.GetRequiredService<IMeilisearchClient>();

        var entries = await db
            .Recipes.AsNoTracking()
            .DeleteFieldNull()
            .Where(r =>
                r.RecipeSearchExtraction == null
                && r.RecipeBook!.Deleted == null
                && r.RecipeBook!.Owner!.Deleted == null
            )
            .Select(r => new
            {
                r.Id,
                r.RecipeBookFk,
                r.Name,
                r.ShortDescription,
                r.SearchVersion,
            })
            .Take(20)
            .ToListAsync(ct);

        if (entries.Count < 1)
        {
            return 0;
        }

        foreach (var entry in entries)
        {
            RecipeSearchExtractionStatusDbObject recipeSearchIndexEntry = new()
            {
                SearchVersion = null,
                RecipeFk = entry.Id,
                RecipeBookFk = entry.RecipeBookFk,
            };
            db.Add(recipeSearchIndexEntry);
        }

        await db.SaveChangesAsync(ct);
        return entries.Count;
    }

    private async Task DeleteRecordBatchAsync(
        ApplicationDbContext db,
        IMeilisearchClient searchClient,
        List<RecipeDatabaseDeletionRow> entries,
        CancellationToken ct
    )
    {
        DeleteDocumentsResponse result = await searchClient.DeleteDocumentsAsync(
            RecipesSearchIndex,
            entries.Select(e => MakeRecipeStringKey(e.RecipeId)),
            ct
        );

        List<long> searchIndexEntries = [.. entries.Select(e => e.SearchEntryId)];
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        await db
            .RecipeSearchExtractionStatusEntries.Where(e => searchIndexEntries.Contains(e.Id))
            .ExecuteUpdateAsync(
                setter =>
                    setter
                        .SetProperty(e => e.TaskUid, result.TaskUid)
                        .SetProperty(e => e.TaskPostTime, now),
                ct
            );

        TaskStatusResponse taskStatus = await searchClient.WaitForTaskCompletionAsync(
            result.TaskUid,
            ct
        );

        if (taskStatus.Status == MeilisearchTaskStatus.Succeeded)
        {
            foreach (var entry in entries)
            {
                await db
                    .RecipeSearchExtractionStatusEntries.Where(e =>
                        e.TaskUid == result.TaskUid && e.Id == entry.SearchEntryId
                    )
                    .ExecuteDeleteAsync(ct);
            }
        }
        else if (
            taskStatus.Status == MeilisearchTaskStatus.Failed
            || taskStatus.Status == MeilisearchTaskStatus.Canceled
        )
        {
            await db
                .RecipeSearchExtractionStatusEntries.Where(e => e.TaskUid == result.TaskUid)
                .ExecuteUpdateAsync(
                    setter =>
                        setter
                            .SetProperty(e => e.TaskUid, (long?)null)
                            .SetProperty(e => e.TaskPostTime, (long?)null),
                    ct
                );
            taskStatus.EnsureSuccess();
        }
        else
        {
            throw new NotImplementedException(
                $"task with id {taskStatus.Uid} has an unexpected status : {taskStatus.Status}"
            );
        }
    }

    private async Task DeleteRecordsAsync(CancellationToken ct)
    {
        await using AsyncServiceScope scope = sp.CreateAsyncScope();
        await using ApplicationDbContext db =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IMeilisearchClient searchClient =
            scope.ServiceProvider.GetRequiredService<IMeilisearchClient>();
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();

        var entries = await db
            .RecipeSearchExtractionStatusEntries.AsNoTracking()
            .Where(e =>
                (
                    e.Recipe!.Deleted != null
                    || e.RecipeBook!.Deleted != null
                    || e.RecipeBook!.Owner!.Deleted != null
                )
                && (e.SearchVersion != null || e.TaskUid != null)
                && (e.NextDeletionTryTime == null || e.NextDeletionTryTime < now)
            )
            .Select(e => new RecipeDatabaseDeletionRow(
                e.RecipeFk,
                e.Id,
                e.NextDeletionTryTime,
                e.DeletionTryCounter
            ))
            .Take(20)
            .ToListAsync(ct);

        if (entries.Count < 1)
        {
            return;
        }

        try
        {
            await DeleteRecordBatchAsync(db, searchClient, entries, ct);
            return;
        }
        catch (Exception)
        {
            // todo - log
        }

        foreach (var entry in entries)
        {
            try
            {
                await DeleteRecordBatchAsync(db, searchClient, [entry], ct);
            }
            catch (Exception)
            {
                // todo - log
                try
                {
                    long currentCount = entry.DeletionTryCounter ?? 0;
                    long newTryCounter = currentCount + 1;
                    if (newTryCounter >= 16)
                    {
                        await db
                            .RecipeSearchExtractionStatusEntries.Where(e =>
                                e.Id == entry.SearchEntryId
                            )
                            .ExecuteDeleteAsync(ct);
                    }
                    else
                    {
                        long nextTryTime = clock
                            .GetCurrentInstant()
                            .Plus(Duration.FromMinutes(Math.Min(2048, Math.Pow(2, currentCount))))
                            .ToUnixTimeSeconds();
                        await db
                            .RecipeSearchExtractionStatusEntries.Where(e =>
                                e.Id == entry.SearchEntryId
                            )
                            .ExecuteUpdateAsync(
                                setter =>
                                    setter
                                        .SetProperty(e => e.DeletionTryCounter, e => newTryCounter)
                                        .SetProperty(e => e.NextDeletionTryTime, nextTryTime),
                                ct
                            );
                    }
                }
                catch (Exception)
                {
                    // todo - log
                }
            }
        }
    }

    /// <summary>
    /// Creates a recipe key for search
    /// </summary>
    /// <param name="id">the id to convert</param>
    /// <returns>the recipe key as a string</returns>
    private static string MakeRecipeStringKey(long id)
    {
        return $"recipe{PaddedLong(id)}";
    }

    /// <summary>
    /// Creates a recipe book key for search
    /// </summary>
    /// <param name="id">the id to convert</param>
    /// <returns>the recipe book key as a string</returns>
    private static string MakeRecipeBookStringKey(long id)
    {
        return $"recipeBook{PaddedLong(id)}";
    }

    /// <summary>
    /// Creates a string padded to 19 places
    /// </summary>
    /// <param name="id">the long to pad</param>
    /// <returns>the padded long as a string</returns>
    private static string PaddedLong(long id)
    {
        return id.ToString("D19", CultureInfo.InvariantCulture);
    }
}
