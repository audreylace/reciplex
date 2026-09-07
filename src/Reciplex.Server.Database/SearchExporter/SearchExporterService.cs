using System.Diagnostics;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
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
    IClock clock,
    IConcurrencyTagProvider concurrencyTagProvider,
    SearchExporterMetrics metrics,
    SearchIndexCreationStrategy searchIndexCreationStrategy
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
    /// Max time in the future a lease may be set to expire to before it is ignored
    /// </summary>
    private const int HourInSeconds = 60 * 60; // 1 hour

    /// <summary>
    /// Max time a lease should be held.
    /// </summary>
    private const int LeaseTime = 5 * 60; // 5 minutes

    /// <summary>
    /// Max times to attempt extraction or deletion
    /// </summary>
    private const int MaxBatchRetries = 17;

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
            if (await searchIndexCreationStrategy.UpsertRecipeIndexAsync(stoppingToken))
            {
                anyWork = await DeleteEmptyRecipeSearchRowsAsync(stoppingToken);
                anyWork |= await DeleteFromSearchIndexAsync(stoppingToken);
                anyWork |= await PopulateEmptySearchEntriesAsync(stoppingToken);
                anyWork |= await ExportChangedRecipesAsync(stoppingToken);
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
    /// Opens a scope and exports changes to Meilisearch
    /// </summary>
    /// <param name="outerCt">async cancellation token</param>
    /// <returns>true if the loop should run again</returns>
    private Task<bool> ExportChangedRecipesAsync(CancellationToken outerCt) =>
        repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            async (db, scope, innerCt) =>
            {
                IMeilisearchClient searchClient =
                    scope.ServiceProvider.GetRequiredService<IMeilisearchClient>();

                long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
                List<(long RecipeFk, string ConcurrencyTag)> candidatesToExtract =
                    await FindRecipesToExtractAsync(db, now, innerCt);
                if (candidatesToExtract.Count < 1)
                {
                    return false;
                }

                string claimTag = concurrencyTagProvider.NextTag();
                long leaseExpireTime = now + LeaseTime;

                int totalClaimed = await ClaimRecipesAsync(
                    db,
                    leaseExpireTime,
                    candidatesToExtract,
                    claimTag,
                    innerCt
                );

                if (totalClaimed < 1) // Didn't claim any. Loop again and re-try.
                {
                    return true;
                }

                List<RecipeRecordDataExtractedFromDatabase> recordData = await ExtractRecipesAsync(
                    db,
                    [.. candidatesToExtract.Select(e => e.RecipeFk)],
                    claimTag,
                    innerCt
                );

                if (recordData.Count < 1)
                {
                    return true;
                }

                try
                {
                    await ExportRecipesToSearchIndexAsync(searchClient, recordData, innerCt);
                }
                catch (Exception batchException)
                    when (batchException is not OperationCanceledException)
                {
                    logger.Error_ExportingBatchToSearchIndex(batchException);
                    await ExtractRecipesInSerialAsync(
                        db,
                        searchClient,
                        claimTag,
                        recordData,
                        innerCt
                    );
                    return true;
                }

                foreach (RecipeRecordDataExtractedFromDatabase singleRecord in recordData)
                {
                    await MarkRecipeExtractAsSuccessNoThrowAsync(
                        db,
                        claimTag,
                        singleRecord.RecipeFk,
                        singleRecord.SearchVersion,
                        false,
                        innerCt
                    );
                }

                return true;
            },
            (success, time) =>
            {
                metrics.ObserveSearchExportOperation(
                    SearchExporterMetrics.RecipesKind,
                    success,
                    time
                );
            },
            logger.Error_UnhandledExceptionWhenSearchExporting,
            outerCt
        );

    /// <summary>
    /// Extracts recipes to the search index one by one
    /// </summary>
    /// <param name="db">the database connection</param>
    /// <param name="searchClient">client for publishing recipes to the search index</param>
    /// <param name="claimTag">the tag used to claim rows</param>
    /// <param name="recordData">the data to extract</param>
    /// <param name="ct">async cancellation token</param>
    private async Task ExtractRecipesInSerialAsync(
        ApplicationDbContext db,
        IMeilisearchClient searchClient,
        string claimTag,
        List<RecipeRecordDataExtractedFromDatabase> recordData,
        CancellationToken ct
    )
    {
        foreach (RecipeRecordDataExtractedFromDatabase singleRecord in recordData)
        {
            bool success = true;
            try
            {
                await ExportRecipesToSearchIndexAsync(searchClient, [singleRecord], ct);
            }
            catch (Exception recipeException)
                when (recipeException is not OperationCanceledException)
            {
                logger.Error_ExportingRecipeToSearchIndex(singleRecord.RecipeFk, recipeException);
                success = false;
            }
            if (success)
            {
                await MarkRecipeExtractAsSuccessNoThrowAsync(
                    db,
                    claimTag,
                    singleRecord.RecipeFk,
                    singleRecord.SearchVersion,
                    true,
                    ct
                );
            }
            else
            {
                await MarkRecipeExtractAsFailedNoThrowAsync(
                    db,
                    singleRecord.RecipeFk,
                    singleRecord.SearchVersion,
                    claimTag,
                    ct
                );
            }
        }
    }

    /// <summary>
    /// Claims recipes for extraction
    /// </summary>
    /// <param name="db">the database connection</param>
    /// <param name="leaseExpireTime">the expire time</param>
    /// <param name="candidateSelection">set of rows to claim</param>
    /// <param name="claimTag">the claim tag for the batch</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows claimed</returns>
    private static async Task<int> ClaimRecipesAsync(
        ApplicationDbContext db,
        long leaseExpireTime,
        List<(long RecipeFk, string ConcurrencyTag)> candidateSelection,
        string claimTag,
        CancellationToken ct
    )
    {
        int claimedCount = 0;
        foreach (var (RecipeFk, ConcurrencyTag) in candidateSelection)
        {
            claimedCount += await db
                .RecipeSearchExtractionStatusEntries.Where(e =>
                    e.RecipeFk == RecipeFk && e.ConcurrencyTag == ConcurrencyTag
                )
                .ExecuteUpdateAsync(
                    s =>
                        s.SetProperty(e => e.LeaseExpireTime, leaseExpireTime)
                            .SetProperty(e => e.ConcurrencyTag, claimTag)
                            .SetProperty(e => e.ExtractionAttempted, true),
                    ct
                );
        }

        return claimedCount;
    }

    /// <summary>
    /// Finds recipes to extract
    /// </summary>
    /// <param name="db">the database connection</param>
    /// <param name="now">the current time</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>the set of recipes to possibly extract</returns>
    private static ValueTask<
        List<(long RecipeFk, string ConcurrencyTag)>
    > FindRecipesToExtractAsync(ApplicationDbContext db, long now, CancellationToken ct) =>
        db
            .RecipeSearchExtractionStatusEntries.AsNoTracking()
            .Where(searchExtractState =>
                // look for records never extracted or have since changed
                (
                    searchExtractState.SearchVersion == null
                    || searchExtractState.Recipe!.SearchVersion > searchExtractState.SearchVersion
                )
                // filter soft deleted records
                && searchExtractState.Recipe!.Deleted == null
                && searchExtractState.Recipe!.RecipeBook!.Deleted == null
                && searchExtractState.Recipe!.RecipeBook!.Owner!.Deleted == null
                // filter out records that are broken and respect retry backoff
                && (
                    (
                        searchExtractState.ErrorCount < MaxBatchRetries
                        && (
                            searchExtractState.NextRetryTime == null
                            || searchExtractState.NextRetryTime < now
                        )
                    )
                    || searchExtractState.Recipe.SearchVersion
                        > searchExtractState.AttemptedExtractSearchVersion
                )
                // filter our records held by other servers with clock skew detection
                && (
                    searchExtractState.LeaseExpireTime == null
                    || searchExtractState.LeaseExpireTime < now
                    || searchExtractState.LeaseExpireTime > now + HourInSeconds
                )
            )
            .Select(e => new { e.RecipeFk, e.ConcurrencyTag })
            .Take(20)
            .ToAsyncEnumerable()
            .Select(e => (e.RecipeFk, e.ConcurrencyTag)) // expression tree's don't support tuples
            .ToListAsync(ct);

    /// <summary>
    /// Marks recipes as extracted with exception handling
    /// </summary>
    /// <param name="db">the database connection</param>
    /// <param name="claimTag">the token used to claim rows</param>
    /// <param name="recipeFk">the id of the recipe to mark as extracted</param>
    /// <param name="searchVersion">the extracted search version</param>
    /// <param name="isRetry">if this was from a retry</param>
    /// <param name="ct">async cancellation token</param>
    private async Task MarkRecipeExtractAsSuccessNoThrowAsync(
        ApplicationDbContext db,
        string claimTag,
        long recipeFk,
        long searchVersion,
        bool isRetry,
        CancellationToken ct
    )
    {
        string outcome = isRetry
            ? SearchExporterMetrics.RowsExportRetrySuccess
            : SearchExporterMetrics.RowsExportSuccess;
        try
        {
            string nextTag = concurrencyTagProvider.NextTag();
            await db
                .RecipeSearchExtractionStatusEntries.Where(searchExtractState =>
                    searchExtractState.RecipeFk == recipeFk
                    && searchExtractState.ConcurrencyTag == claimTag
                )
                .ExecuteUpdateAsync(
                    s =>
                        s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                            .SetProperty(e => e.ConcurrencyTag, nextTag)
                            .SetProperty(e => e.ErrorCount, 0)
                            .SetProperty(e => e.NextRetryTime, (long?)null)
                            .SetProperty(e => e.SearchVersion, searchVersion)
                            .SetProperty(e => e.AttemptedExtractSearchVersion, (long?)null),
                    ct
                );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.Error_UnhandledExceptionWhenMarkingRecordAsExtracted(
                recipeFk,
                searchVersion,
                ex
            );
            outcome = SearchExporterMetrics.RowsExportDatabaseErrorSuccess;
        }

        metrics.ObserveRecipesExported(SearchExporterMetrics.RecipesKind, 1, outcome);
    }

    /// <summary>
    /// Exports a set of recipes as a single batch to the search index
    /// </summary>
    /// <param name="searchClient">client to run the extract operation</param>
    /// <param name="entries">set of entries to publish</param>
    /// <param name="ct">async cancellation token</param>
    private static async Task ExportRecipesToSearchIndexAsync(
        IMeilisearchClient searchClient,
        List<RecipeRecordDataExtractedFromDatabase> entries,
        CancellationToken ct
    )
    {
        MeilisearchTaskResponse result = await searchClient.UpsertDocumentsAsync(
            RecipesSearchIndex,
            entries.Select(e => new RecipeSearchIndexEntry()
            {
                Id = MakeRecipeStringKey(e.RecipeFk),
                Name = e.Name,
                ShortDescription = e.ShortDescription,
                BookId = MakeRecipeBookStringKey(e.RecipeBookFk),
            }),
            ct
        );

        TaskStatusResponse? taskStatus = await searchClient.WaitForTaskCompletionAsync(
            result.TaskUid,
            ct
        );

        if (taskStatus?.Status != MeilisearchTaskStatus.Succeeded)
        {
            TaskStatusResponse.ThrowIfNotSuccess(taskStatus);
        }
    }

    /// <summary>
    /// Extract recipes from the database for extraction to the search index
    /// </summary>
    /// <param name="db">the database connection</param>
    /// <param name="ids">the set of ids to extract</param>
    /// <param name="concurrencyTag">the concurrency token set on the claimed rows</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>the extracted data</returns>
    private static Task<List<RecipeRecordDataExtractedFromDatabase>> ExtractRecipesAsync(
        ApplicationDbContext db,
        List<long> ids,
        string concurrencyTag,
        CancellationToken ct
    ) =>
        db
            .RecipeSearchExtractionStatusEntries.AsNoTracking()
            .Where(searchExtractState =>
                ids.Contains(searchExtractState.RecipeFk)
                && searchExtractState.ConcurrencyTag == concurrencyTag
            )
            .Select(r => new RecipeRecordDataExtractedFromDatabase(
                r.RecipeFk,
                r.Recipe!.Name,
                r.Recipe!.ShortDescription,
                r.Recipe!.RecipeBookFk,
                r.Recipe!.SearchVersion
            ))
            .ToListAsync(ct);

    /// <summary>
    /// Extracted recipe data from the database
    /// </summary>
    /// <param name="RecipeFk">the id of the recipe</param>
    /// <param name="Name">the recipe name</param>
    /// <param name="ShortDescription">the recipe short description</param>
    /// <param name="RecipeBookFk">the recipe book id</param>
    /// <param name="SearchVersion">the recipe search version</param>
    record class RecipeRecordDataExtractedFromDatabase(
        long RecipeFk,
        string Name,
        string ShortDescription,
        long RecipeBookFk,
        long SearchVersion
    );

    /// <summary>
    /// Marks recipe extraction failed
    /// </summary>
    /// <param name="db">the database connection</param>
    /// <param name="recipeFk">the recipe id to mark as failed</param>
    /// <param name="searchVersion">the recipe search version</param>
    /// <param name="claimTag">the tag used to claim the rows</param>
    /// <param name="ct">async cancellation token</param>
    private async Task MarkRecipeExtractAsFailedNoThrowAsync(
        ApplicationDbContext db,
        long recipeFk,
        long searchVersion,
        string claimTag,
        CancellationToken ct
    )
    {
        string outcome = SearchExporterMetrics.RowsExportRetryFailed;
        try
        {
            long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
            string nextTag = concurrencyTagProvider.NextTag();
            await db
                .RecipeSearchExtractionStatusEntries.Where(searchExtractState =>
                    searchExtractState.RecipeFk == recipeFk
                    && searchExtractState.ConcurrencyTag == claimTag
                )
                .ExecuteUpdateAsync(
                    s =>
                        s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                            .SetProperty(e => e.ConcurrencyTag, nextTag)
                            .SetProperty(e => e.NextRetryTime, e => now + (1 << e.ErrorCount))
                            .SetProperty(e => e.ErrorCount, e => e.ErrorCount + 1)
                            .SetProperty(e => e.AttemptedExtractSearchVersion, searchVersion),
                    ct
                );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.Error_IncrementingExtractionAttemptCounter(recipeFk, searchVersion, ex);
            outcome = SearchExporterMetrics.RowsExportDatabaseErrorFailure;
        }

        metrics.ObserveRecipesExported(SearchExporterMetrics.RecipesKind, 1, outcome);
    }

    /// <summary>
    /// Creates empty search entries for new records but does not extract them.
    /// </summary>
    /// <param name="outerCt">async cancellation token</param>
    /// <returns>number of new entries created</returns>
    /// <remarks>
    /// We use the <see cref="RecipeSearchWorkerStateDbObject" />
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
                    .Select(r => new { r.Id })
                    .Take(20)
                    .ToListAsync(innerCt);

                if (entries.Count < 1)
                {
                    return false;
                }

                long counter = 0;
                foreach (var entry in entries)
                {
                    string tag = concurrencyTagProvider.NextTag();
                    RecipeSearchWorkerStateDbObject recipeSearchIndexEntry = new()
                    {
                        SearchVersion = null,
                        RecipeFk = entry.Id,
                        ConcurrencyTag = tag,
                    };
                    db.Add(recipeSearchIndexEntry);
                    counter++;
                }

                await db.SaveChangesAsync(innerCt);
                metrics.ObserveEmptyRowCreation(SearchExporterMetrics.RecipesKind, counter);
                return true;
            },
            (success, duration) =>
            {
                metrics.ObserveEmptyRowCreationDuration(
                    SearchExporterMetrics.RecipesKind,
                    success,
                    duration
                );
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
    private Task<bool> DeleteEmptyRecipeSearchRowsAsync(CancellationToken outerCt) =>
        repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            DeletionDatabaseActionStrategy.CollectAndDelete(
                (db, innerCt) =>
                    db
                        .RecipeSearchExtractionStatusEntries.Where(e =>
                            (
                                e.Recipe!.Deleted != null
                                || e.Recipe.RecipeBook!.Deleted != null
                                || e.Recipe!.RecipeBook.Owner!.Deleted != null
                            )
                            && e.ExtractionAttempted == false
                        )
                        .Select(e => e.RecipeFk)
                        .Take(25)
                        .ToListAsync(innerCt),
                (db, ids, innerCt) =>
                    db
                        .RecipeSearchExtractionStatusEntries.Where(e =>
                            ids.Contains(e.RecipeFk) && !e.ExtractionAttempted
                        )
                        .ExecuteDeleteAsync(innerCt),
                (collectTime, deleteTime, rowCount) =>
                {
                    metrics.ObserveEmptySearchRowDeletion(
                        SearchExporterMetrics.RecipesKind,
                        rowCount,
                        collectTime,
                        deleteTime
                    );
                }
            ),
            (success, time) =>
            {
                metrics.ObserveEmptyRowDeletionDuration(
                    SearchExporterMetrics.RecipesKind,
                    success,
                    time
                );
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
                        && (e.NextDTryTime == null || e.NextDTryTime < now)
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
