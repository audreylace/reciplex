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
                anyWork |= await BreakRecipeLeaseEntriesAsync(stoppingToken);
                anyWork |= await DeleteStuckRecipeEntriesAsync(stoppingToken);
                anyWork |= await DeleteEmptyRecipeSearchRowsAsync(stoppingToken);
                anyWork |= await DeleteFromSearchIndexAsync(stoppingToken);
                anyWork |= await PopulateEmptySearchEntriesAsync(stoppingToken);
                anyWork |= await FindAndExportRecipesAsync(stoppingToken);
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
    private Task<bool> FindAndExportRecipesAsync(CancellationToken outerCt) =>
        repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            CollectActDatabaseActionStrategy.CollectAndAct(
                async (db, scope, innerCt) => await FindRecipesToExportAsync(db, innerCt),
                ExportRecipesAsync,
                (_, _, _) => { }
            ),
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

    private async Task<int> ExportRecipesAsync(
        ApplicationDbContext db,
        AsyncServiceScope scope,
        List<(long RecipeFk, string ConcurrencyTag)> candidatesToExtract,
        CancellationToken innerCt
    )
    {
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        IMeilisearchClient searchClient =
            scope.ServiceProvider.GetRequiredService<IMeilisearchClient>();
        string claimTag = concurrencyTagProvider.NextTag();
        long leaseExpireTime = now + LeaseTime;

        int totalClaimed = await ClaimRecipesAsync(
            db,
            leaseExpireTime,
            candidatesToExtract,
            claimTag,
            true,
            innerCt
        );

        if (totalClaimed < 1)
        {
            return 0;
        }

        List<RecipeRecordDataExtractedFromDatabase> recordData = await GetRecipeDataForExportAsync(
            db,
            [.. candidatesToExtract.Select(e => e.RecipeFk)],
            claimTag,
            innerCt
        );

        if (recordData.Count < 1)
        {
            return 0;
        }

        try
        {
            await ExportRecipesToSearchIndexAsync(searchClient, recordData, innerCt);
        }
        catch (Exception batchException) when (batchException is not OperationCanceledException)
        {
            logger.Error_ExportingBatchToSearchIndex(batchException);
            await ExportRecipesInSerialAsync(db, searchClient, claimTag, recordData, innerCt);
            return recordData.Count;
        }

        foreach (RecipeRecordDataExtractedFromDatabase singleRecord in recordData)
        {
            await MarkRecipeExportAsSuccessNoThrowAsync(
                db,
                claimTag,
                singleRecord.RecipeFk,
                singleRecord.SearchVersion,
                false,
                innerCt
            );
        }

        return recordData.Count;
    }

    /// <summary>
    /// Extracts recipes to the search index one by one
    /// </summary>
    /// <param name="db">the database connection</param>
    /// <param name="searchClient">client for publishing recipes to the search index</param>
    /// <param name="claimTag">the tag used to claim rows</param>
    /// <param name="recordData">the data to extract</param>
    /// <param name="ct">async cancellation token</param>
    private async Task ExportRecipesInSerialAsync(
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
                await MarkRecipeExportAsSuccessNoThrowAsync(
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
                await MarkRecipeExportAsFailedNoThrowAsync(
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
    /// Finds recipes to extract
    /// </summary>
    /// <param name="db">the database connection</param>
    /// <param name="now">the current time</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>the set of recipes to possibly extract</returns>
    private async ValueTask<List<(long RecipeFk, string ConcurrencyTag)>> FindRecipesToExportAsync(
        ApplicationDbContext db,
        CancellationToken ct
    )
    {
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        return await db
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
                        searchExtractState.ExtractRetryCount < MaxBatchRetries
                        && (
                            searchExtractState.NextExtractRetryTime == null
                            || searchExtractState.NextExtractRetryTime < now
                        )
                    )
                    || searchExtractState.Recipe.SearchVersion
                        > searchExtractState.AttemptedExtractSearchVersion
                )
                && searchExtractState.LeaseExpireTime == null // todo - lease breaker
            )
            .Select(e => new { e.RecipeFk, e.ConcurrencyTag })
            .Take(20)
            .ToAsyncEnumerable()
            .Select(e => (e.RecipeFk, e.ConcurrencyTag)) // expression tree's don't support tuples
            .ToListAsync(ct);
    }

    /// <summary>
    /// Marks recipes as extracted with exception handling
    /// </summary>
    /// <param name="db">the database connection</param>
    /// <param name="claimTag">the token used to claim rows</param>
    /// <param name="recipeFk">the id of the recipe to mark as extracted</param>
    /// <param name="searchVersion">the extracted search version</param>
    /// <param name="isRetry">if this was from a retry</param>
    /// <param name="ct">async cancellation token</param>
    private async Task MarkRecipeExportAsSuccessNoThrowAsync(
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
                            .SetProperty(e => e.ExtractRetryCount, 0)
                            .SetProperty(e => e.NextExtractRetryTime, (long?)null)
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
        TaskStatusResponse? taskStatus = await searchClient.UpsertDocumentsAndWaitAsync(
            RecipesSearchIndex,
            entries.Select(e => new RecipeSearchIndexEntry()
            {
                Id = RecordIdAsStringForSearch.MakeRecipeStringKey(e.RecipeFk),
                Name = e.Name,
                ShortDescription = e.ShortDescription,
                BookId = RecordIdAsStringForSearch.MakeRecipeBookStringKey(e.RecipeBookFk),
            }),
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
    private static Task<List<RecipeRecordDataExtractedFromDatabase>> GetRecipeDataForExportAsync(
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
    private async Task MarkRecipeExportAsFailedNoThrowAsync(
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
                            .SetProperty(
                                e => e.NextExtractRetryTime,
                                e => now + (1 << e.ExtractRetryCount)
                            )
                            .SetProperty(e => e.ExtractRetryCount, e => e.ExtractRetryCount + 1)
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
    /// <returns>true if any work was done</returns>
    private Task<bool> PopulateEmptySearchEntriesAsync(CancellationToken outerCt)
    {
        return repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            CollectActDatabaseActionStrategy.CollectAndAct(
                (db, innerCt) =>
                    db
                        .Recipes.AsNoTracking()
                        .DeleteFieldNull()
                        .Where(r =>
                            r.RecipeSearchExtraction == null
                            && r.RecipeBook!.Deleted == null
                            && r.RecipeBook!.Owner!.Deleted == null
                        )
                        .Select(r => r.Id)
                        .Take(20)
                        .ToListAsync(innerCt),
                async (db, entries, innerCt) =>
                {
                    foreach (var id in entries)
                    {
                        string tag = concurrencyTagProvider.NextTag();
                        RecipeSearchWorkerStateDbObject recipeSearchIndexEntry = new()
                        {
                            SearchVersion = null,
                            RecipeFk = id,
                            ConcurrencyTag = tag,
                        };
                        db.Add(recipeSearchIndexEntry);
                    }
                    return await db.SaveChangesAsync(innerCt);
                },
                (_, _, rows) =>
                {
                    metrics.ObserveEmptyRowCreation(SearchExporterMetrics.RecipesKind, rows);
                }
            ),
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
            CollectActDatabaseActionStrategy.CollectAndAct(
                (db, innerCt) =>
                    db
                        .RecipeSearchExtractionStatusEntries.Where(e =>
                            (
                                e.Recipe!.Deleted != null
                                || e.Recipe.RecipeBook!.Deleted != null
                                || e.Recipe!.RecipeBook.Owner!.Deleted != null
                            )
                            && e.ExtractionAttempted == false
                            && e.LeaseExpireTime == null
                        )
                        .Select(e => e.RecipeFk)
                        .Take(25)
                        .ToListAsync(innerCt),
                (db, ids, innerCt) =>
                    db
                        .RecipeSearchExtractionStatusEntries.Where(e =>
                            ids.Contains(e.RecipeFk)
                            && !e.ExtractionAttempted
                            && e.LeaseExpireTime == null
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
                List<(long RecipeFk, string ConcurrencyTag)> candidatesToDelete =
                    await FindRecipeCandidatesToDelete(db, now, innerCt);

                if (candidatesToDelete.Count < 1)
                {
                    return false;
                }

                long leaseExpireTime = now + LeaseTime;
                string claimTag = concurrencyTagProvider.NextTag();
                int totalClaimed = await ClaimRecipesAsync(
                    db,
                    leaseExpireTime,
                    candidatesToDelete,
                    claimTag,
                    false,
                    innerCt
                );

                if (totalClaimed < 1) // Didn't claim any. Loop again and re-try.
                {
                    return true;
                }

                try
                {
                    await DeleteRecipesFromSearchIndexAsync(
                        searchClient,
                        candidatesToDelete.Select(c => c.RecipeFk),
                        innerCt
                    );
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // todo - log
                    foreach (var (RecipeFk, ConcurrencyTag) in candidatesToDelete)
                    {
                        bool success;
                        try
                        {
                            await DeleteRecipesFromSearchIndexAsync(
                                searchClient,
                                [RecipeFk],
                                innerCt
                            );
                            success = true;
                        }
                        catch (Exception innerEx) when (innerEx is not OperationCanceledException)
                        {
                            success = false;
                            // todo - log
                        }

                        if (success)
                        {
                            await DeleteSearchRecipeSearchRecordNoThrowAsync(
                                db,
                                RecipeFk,
                                claimTag,
                                innerCt
                            );
                        }
                        else
                        {
                            await MarkRecipeDeleteAsFailedNoThrowAsync(
                                db,
                                RecipeFk,
                                claimTag,
                                innerCt
                            );
                        }
                    }

                    return true;
                }

                foreach ((long RecipeFk, string ConcurrencyTag) in candidatesToDelete)
                {
                    await DeleteSearchRecipeSearchRecordNoThrowAsync(
                        db,
                        RecipeFk,
                        claimTag,
                        innerCt
                    );
                }

                return true;
            },
            (_, _) => {
                /** metrics */
            },
            logger.Error_DeleteFromSearchIndex,
            outerCt
        );

    private static async Task<
        List<(long RecipeFk, string ConcurrencyTag)>
    > FindRecipeCandidatesToDelete(ApplicationDbContext db, long now, CancellationToken innerCt)
    {
        return await db
            .RecipeSearchExtractionStatusEntries.AsNoTracking()
            .Where(e =>
                (
                    e.Recipe!.Deleted != null
                    || e.Recipe.RecipeBook!.Deleted != null
                    || e.Recipe.RecipeBook!.Owner!.Deleted != null
                )
                && e.ExtractionAttempted
                && e.LeaseExpireTime == null
                && e.DeleteRetryCounter < MaxBatchRetries
                && (e.NextDeleteRetryTime == null || e.NextDeleteRetryTime < now)
            )
            .Select(e => new { e.ConcurrencyTag, e.RecipeFk })
            .Take(20)
            .AsAsyncEnumerable()
            .Select(e => (e.RecipeFk, e.ConcurrencyTag))
            .ToListAsync(innerCt);
    }

    private async Task MarkRecipeDeleteAsFailedNoThrowAsync(
        ApplicationDbContext db,
        long recipeFk,
        string claimTag,
        CancellationToken ct
    )
    {
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
                            .SetProperty(
                                e => e.NextDeleteRetryTime,
                                e => now + (1 << e.DeleteRetryCounter)
                            )
                            .SetProperty(e => e.DeleteRetryCounter, e => e.DeleteRetryCounter + 1),
                    ct
                );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // log
        }
    }

    private static async Task DeleteSearchRecipeSearchRecordNoThrowAsync(
        ApplicationDbContext db,
        long recipeFk,
        string claimTag,
        CancellationToken ct
    )
    {
        try
        {
            await db
                .RecipeSearchExtractionStatusEntries.Where(searchExtractState =>
                    searchExtractState.RecipeFk == recipeFk
                    && searchExtractState.ConcurrencyTag == claimTag
                )
                .ExecuteDeleteAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // log
        }
    }

    /// <summary>
    /// Deletes a set of records from the search index in a single batch operation
    /// </summary>
    /// <param name="searchClient">the search client</param>
    /// <param name="entries">the set of entries to delete</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>true if the operation succeeded</returns>
    private static async Task DeleteRecipesFromSearchIndexAsync(
        IMeilisearchClient searchClient,
        IEnumerable<long> entries,
        CancellationToken ct
    )
    {
        MeilisearchTaskResponse result = await searchClient.DeleteDocumentsAsync(
            RecipesSearchIndex,
            entries.Select(id => RecordIdAsStringForSearch.MakeRecipeStringKey(id)),
            ct
        );

        TaskStatusResponse? taskStatus = await searchClient.WaitForTaskCompletionAsync(
            result.TaskUid,
            ct
        );

        TaskStatusResponse.ThrowIfNotSuccess(taskStatus);
    }

    #endregion Deletion


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
        bool extractionAttemptFlagOr,
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
                            .SetProperty(
                                e => e.ExtractionAttempted,
                                e => e.ExtractionAttempted || extractionAttemptFlagOr
                            ),
                    ct
                );
        }

        return claimedCount;
    }

    private Task<bool> BreakRecipeLeaseEntriesAsync(CancellationToken outerCt)
    {
        return repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            CollectActDatabaseActionStrategy.CollectAndAct(
                async (db, innerCt) =>
                {
                    long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
                    return await db
                        .RecipeSearchExtractionStatusEntries.Where(e =>
                            e.LeaseExpireTime != null
                            && (e.LeaseExpireTime < now || e.LeaseExpireTime > (now + LeaseTime))
                        )
                        .Select(e => new { e.RecipeFk, e.ConcurrencyTag })
                        .Take(20)
                        .ToListAsync(innerCt);
                },
                async (db, entries, innerCt) =>
                {
                    int claimedCount = 0;
                    foreach (var entry in entries)
                    {
                        string claimTag = concurrencyTagProvider.NextTag();
                        claimedCount += await db
                            .RecipeSearchExtractionStatusEntries.Where(e =>
                                e.RecipeFk == entry.RecipeFk
                                && e.ConcurrencyTag == entry.ConcurrencyTag
                            )
                            .ExecuteUpdateAsync(
                                s =>
                                    s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                                        .SetProperty(e => e.ConcurrencyTag, claimTag),
                                innerCt
                            );
                    }

                    return claimedCount;
                },
                (
                    _,
                    _,
                    _
                ) => { /* metrics */
                }
            ),
            (_, _) => {
                /* metrics */
            },
            ex =>
            { /* error logging */
            },
            outerCt
        );
    }

    private Task<bool> DeleteStuckRecipeEntriesAsync(CancellationToken outerCt)
    {
        return repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            CollectActDatabaseActionStrategy.CollectAndAct(
                (db, innerCt) =>
                    db
                        .RecipeSearchExtractionStatusEntries.Where(e =>
                            e.LeaseExpireTime == null && e.DeleteRetryCounter >= MaxBatchRetries
                        )
                        .Select(e => e.RecipeFk)
                        .Take(20)
                        .ToListAsync(innerCt),
                (db, entries, innerCt) =>
                    db
                        .RecipeSearchExtractionStatusEntries.Where(e =>
                            entries.Contains(e.RecipeFk)
                            && e.DeleteRetryCounter >= MaxBatchRetries
                            && e.LeaseExpireTime == null
                        )
                        .ExecuteDeleteAsync(innerCt),
                (
                    _,
                    _,
                    _
                ) => { /* metrics */
                }
            ),
            (_, _) => {
                /* metrics */
            },
            ex =>
            { /* error logging */
            },
            outerCt
        );
    }
}
