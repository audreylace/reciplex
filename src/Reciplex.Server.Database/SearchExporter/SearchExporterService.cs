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

    /// <summary>
    /// The primary key property name
    /// </summary>
    private const string PrimaryKeyPropertyName = "id";

    /// <summary>
    /// If the recipe index exists
    /// </summary>
    private bool _recipeIndexExists;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int backoff = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            backoff = Math.Min(13, backoff + 1);
            bool anyWork = false;
            if (await UpsertRecipeIndexAsync(stoppingToken))
            {
                anyWork = await DeleteNullSearchPointersAsync(stoppingToken);
                anyWork |= await DeleteFromSearchIndexAsync(stoppingToken);
                anyWork |= await PopulateEmptySearchEntriesAsync(stoppingToken);
                anyWork |= await ExportChangedRecordsAsync(stoppingToken);
            }
            if (anyWork)
            {
                backoff = 1;
            }

            double seconds = Math.Min(3600, Math.Pow(2, backoff - 1));
            await Task.Delay(TimeSpan.FromSeconds(seconds), stoppingToken);
        }
    }

    /// <summary>
    /// Creates the recipe index if it does not already exist
    /// </summary>
    /// <param name="ct">async cancellation token</param>
    /// <returns>true if the index exists</returns>
    private async Task<bool> UpsertRecipeIndexAsync(CancellationToken ct)
    {
        if (_recipeIndexExists)
        {
            return true;
        }
        using var scope = sp.CreateAsyncScope();
        if (
            await UpsertIndexAsync(
                scope.ServiceProvider.GetRequiredService<IMeilisearchClient>(),
                RecipesSearchIndex,
                PrimaryKeyPropertyName,
                ct
            )
        )
        {
            _recipeIndexExists = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Creates an index with name <paramref name="indexName"/> and <paramref name="primaryKey"/>
    /// if it does not already exist.
    /// </summary>
    /// <param name="searchClient">the http client for the remote search server</param>
    /// <param name="indexName">the name of the index</param>
    /// <param name="primaryKey">the primary key of the index</param>
    /// <param name="ct">the async cancellation token</param>
    private async Task<bool> UpsertIndexAsync(
        IMeilisearchClient searchClient,
        string indexName,
        string primaryKey,
        CancellationToken ct
    )
    {
        try
        {
            GetIndexResponse? index = await searchClient.GetIndexAsync(indexName, ct);
            if (index is null)
            {
                MeilisearchTaskResponse createResponse = await searchClient.CreateIndexAsync(
                    indexName,
                    primaryKey,
                    ct
                );

                TaskStatusResponse? createTask = await searchClient.WaitForTaskCompletionAsync(
                    createResponse.TaskUid,
                    ct
                );

                if (createTask?.Status != MeilisearchTaskStatus.Succeeded)
                {
                    logger.Error_IndexCreationTaskFailed(
                        indexName,
                        createTask?.Uid,
                        createTask?.Status
                    );
                    return false;
                }
            }
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.Error_IndexCreationFailedWithException(indexName, primaryKey, ex);
            return false;
        }
    }

    /// <summary>
    /// Opens a scope and exports changes to Meilisearch
    /// </summary>
    /// <param name="outerCt">async cancellation token</param>
    /// <returns>true if any work was done</returns>
    private Task<bool> ExportChangedRecordsAsync(CancellationToken outerCt) =>
        repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            async (db, scope, innerCt) =>
            {
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
                    .ToListAsync(innerCt);

                if (entries.Count < 1)
                {
                    return false;
                }

                if (await ExportRecordBatchAsync(db, searchClient, entries, innerCt))
                {
                    return true;
                }

                bool anyWork = false;
                foreach (RecipeDatabaseExtractionRow entry in entries)
                {
                    if (!await ExportRecordBatchAsync(db, searchClient, [entry], innerCt))
                    {
                        try
                        {
                            await db
                                .RecipeSearchExtractionStatusEntries.Where(e =>
                                    e.Id == entry.SearchEntryId
                                )
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
                                            .SetProperty(
                                                e => e.ErrorSearchVersion,
                                                entry.SearchVersion
                                            ),
                                    innerCt
                                );
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            logger.Error_IncrementingExtractionAttemptCounter(
                                entry.SearchEntryId,
                                entry.RecipeId,
                                ex
                            );
                        }
                    }
                    else
                    {
                        anyWork = true;
                    }
                }

                return anyWork;
            },
            (
                _,
                _
            ) => { /* metrics */
            },
            logger.Error_UnhandledExceptionWhenSearchExporting,
            outerCt
        );

    /// <summary>
    /// Exports a set of records to the search index
    /// </summary>
    /// <param name="db">the database connection</param>
    /// <param name="searchClient">the search client</param>
    /// <param name="entries">set of entries to export</param>
    /// <param name="ct">async token cancellation</param>
    /// <returns>if work was done</returns>
    private async Task<bool> ExportRecordBatchAsync(
        ApplicationDbContext db,
        IMeilisearchClient searchClient,
        List<RecipeDatabaseExtractionRow> entries,
        CancellationToken ct
    )
    {
        try
        {
            MeilisearchTaskResponse result = await searchClient.UpsertDocumentsAsync(
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

            TaskStatusResponse? taskStatus = await searchClient.WaitForTaskCompletionAsync(
                result.TaskUid,
                ct
            );

            if (taskStatus?.Status != MeilisearchTaskStatus.Succeeded)
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
                logger.Error_ExtractionTaskFailed(taskStatus?.Uid, taskStatus?.Status);
                return false;
            }

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
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.Error_ExportToSearchIndex(ex);
            return false;
        }
    }

    /// <summary>
    /// Creates empty search entries for new records but does not extract them.
    /// </summary>
    /// <param name="outerCt">async cancellation token</param>
    /// <returns>number of new entries created</returns>
    /// <remarks>
    /// We use the <see cref="RecipeSearchExtractionStatusDbObject" />
    /// object to know if there could have been an extraction.
    /// If no entry exists for a record, then we know it
    /// was never extracted. If a record happens to exist,
    /// then we know we need to check first before deleting.
    /// </remarks>
    private Task<bool> PopulateEmptySearchEntriesAsync(CancellationToken outerCt)
    {
        return repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            async (db, scope, innerCt) =>
            {
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
                    .ToListAsync(innerCt);

                if (entries.Count < 1)
                {
                    return false;
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

                await db.SaveChangesAsync(innerCt);
                return true;
            },
            (
                _,
                _
            ) => { /* metrics */
            },
            logger.Error_RunningSearchEntryCreation,
            outerCt
        );
    }

    #region Deletion

    /// <summary>
    /// Deletes extraction entries for records that were never extracted
    /// to the search index.
    /// </summary>
    /// <param name="outerCt">async cancellation token</param>
    /// <returns>task that resolves to true if work was done</returns>
    private Task<bool> DeleteNullSearchPointersAsync(CancellationToken outerCt) =>
        repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            DeletionDatabaseActionStrategy.CollectAndDelete(
                (db, innerCt) =>
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
                        .ToListAsync(innerCt),
                (db, ids, innerCt) =>
                    db
                        .RecipeSearchExtractionStatusEntries.Where(e =>
                            ids.Contains(e.Id) && e.SearchVersion == null && e.TaskUid == null
                        )
                        .ExecuteDeleteAsync(innerCt),
                (
                    collectTime,
                    deleteTime,
                    rowCount
                ) => { /* todo - metrics */
                }
            ),
            (
                success,
                time
            ) => { /* todo - metrics */
            },
            logger.Error_UnhandledExceptionDeletingEmptySearchRecords,
            outerCt
        );

    /// <summary>
    /// Deletes entries from the search index when the source records
    /// are marked as deleted.
    /// </summary>
    /// <param name="outerCt">async cancellation token</param>
    /// <returns>true if any work was done</returns>
    private Task<bool> DeleteFromSearchIndexAsync(CancellationToken outerCt) =>
        repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            async (db, scope, innerCt) =>
            {
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
                    .ToListAsync(innerCt);

                if (entries.Count < 1)
                {
                    return false;
                }

                if (await DeleteSearchRecordAsBatchAsync(db, searchClient, entries, innerCt))
                {
                    return true;
                }

                bool someWorkDone = false;
                foreach (var entry in entries)
                {
                    if (!await DeleteSearchRecordAsBatchAsync(db, searchClient, [entry], innerCt))
                    {
                        someWorkDone |= await IncrementDeletionFailedCounterAsync(
                            db,
                            entry,
                            innerCt
                        );
                    }
                }
                return someWorkDone;
            },
            (_, _) => {
                /** metrics */
            },
            logger.Error_DeleteFromSearchIndex,
            outerCt
        );

    /// <summary>
    /// Increments the deletion failed counter and set the next delete time.
    /// If the record is at max attempts, drops it.
    /// </summary>
    /// <param name="db">database connection</param>
    /// <param name="entry">the deletion meta data</param>
    /// <param name="ct">async cancellation token</param>
    private async Task<bool> IncrementDeletionFailedCounterAsync(
        ApplicationDbContext db,
        RecipeDatabaseDeletionRow entry,
        CancellationToken ct
    )
    {
        try
        {
            long currentCount = entry.DeletionTryCounter ?? 0;
            long newTryCounter = currentCount + 1;
            if (newTryCounter >= 16)
            {
                await db
                    .RecipeSearchExtractionStatusEntries.Where(e => e.Id == entry.SearchEntryId)
                    .ExecuteDeleteAsync(ct);
            }
            else
            {
                long nextTryTime = clock
                    .GetCurrentInstant()
                    .Plus(Duration.FromMinutes(Math.Min(2048, Math.Pow(2, currentCount))))
                    .ToUnixTimeSeconds();
                await db
                    .RecipeSearchExtractionStatusEntries.Where(e => e.Id == entry.SearchEntryId)
                    .ExecuteUpdateAsync(
                        setter =>
                            setter
                                .SetProperty(e => e.DeletionTryCounter, e => newTryCounter)
                                .SetProperty(e => e.NextDeletionTryTime, nextTryTime),
                        ct
                    );
            }
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.Error_IncrementingFailedIndexDeletionCounter(
                entry.SearchEntryId,
                entry.RecipeId,
                ex
            );
            return false;
        }
    }

    /// <summary>
    /// Deletes a set of records from the search index in a single batch operation
    /// </summary>
    /// <param name="db">database connection</param>
    /// <param name="searchClient">the search client</param>
    /// <param name="entries">the set of entries to delete</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>true if the operation succeeded</returns>
    private async Task<bool> DeleteSearchRecordAsBatchAsync(
        ApplicationDbContext db,
        IMeilisearchClient searchClient,
        List<RecipeDatabaseDeletionRow> entries,
        CancellationToken ct
    )
    {
        try
        {
            MeilisearchTaskResponse result = await searchClient.DeleteDocumentsAsync(
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

            TaskStatusResponse? taskStatus = await searchClient.WaitForTaskCompletionAsync(
                result.TaskUid,
                ct
            );

            if (taskStatus?.Status != MeilisearchTaskStatus.Succeeded)
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

                logger.Error_DeletionTaskFailed(taskStatus?.Uid, taskStatus?.Status);
                return false;
            }

            foreach (var entry in entries)
            {
                await db
                    .RecipeSearchExtractionStatusEntries.Where(e =>
                        e.TaskUid == result.TaskUid && e.Id == entry.SearchEntryId
                    )
                    .ExecuteDeleteAsync(ct);
            }

            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.Error_DeletingBatchOfRecordsFromSearchIndex(ex);
            return false;
        }
    }

    #endregion Deletion

    #region String Id Helpers

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

    #endregion String Id Helpers
}
