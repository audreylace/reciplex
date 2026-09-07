using System.Diagnostics;
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

/// <summary>
/// Exports recipes to the search index
/// </summary>
/// <param name="logger">logger for this service</param>
/// <param name="repeatedDatabaseActionStrategy">strategy for running repeated database actions</param>
/// <param name="clock">clock for getting the current time</param>
/// <param name="concurrencyTagProvider">concurrency tag provider</param>
/// <param name="metrics">metrics for the service</param>
/// <param name="searchIndexCreationStrategy">strategy for making a search index</param>
/// <remarks>
///  TODO -
///    [ ] Set proper fields on index
/// </remarks>
internal sealed class RecipeSearchExporterService(
    ILogger<RecipeSearchExporterService> logger,
    RepeatedDatabaseActionStrategy repeatedDatabaseActionStrategy,
    IClock clock,
    IConcurrencyTagProvider concurrencyTagProvider,
    RecipeSearchExporterMetrics metrics,
    SearchIndexCreationStrategy searchIndexCreationStrategy
) : BackgroundService
{
    /// <summary>
    /// Recipe search index
    /// </summary>
    private const string RecipesSearchIndex = "recipes";

    /// <summary>
    /// Max time a lease should be held.
    /// </summary>
    private const int LeaseTime = 5 * 60; // 5 minutes

    /// <summary>
    /// Max times to attempt extraction or deletion
    /// </summary>
    private const int MaxBatchRetries = 17;

    /// <summary>
    /// A value greater then 0 to trigger the collect and execute loop running again
    /// </summary>
    private const int LoopAgain = 1;

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
            if (!_recipeIndexExists)
            {
                _recipeIndexExists = await searchIndexCreationStrategy.UpsertRecipeIndexAsync(
                    stoppingToken
                );
            }
            if (_recipeIndexExists)
            {
                // todo - support wake
                anyWork |= await BreakLeaseEntriesAsync(stoppingToken);
                anyWork |= await DeleteStuckEntriesAsync(stoppingToken);
                anyWork |= await DeleteEmptyRowsAsync(stoppingToken);
                anyWork |= await DeleteFromSearchIndexAsync(stoppingToken);
                anyWork |= await PopulateEmptyRowsAsync(stoppingToken);
                anyWork |= await FindAndExportAsync(stoppingToken);
            }
            if (anyWork)
            {
                backoff = 1;
            }

            double seconds = Math.Min(3600, Math.Pow(2, backoff - 1));
            await Task.Delay(TimeSpan.FromSeconds(seconds), stoppingToken);
        }
    }

    private Task<bool> FindAndExportAsync(CancellationToken outerCt) =>
        repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            async (db, scope, innerCt) =>
            {
                List<(long RecipeFk, string ConcurrencyTag)> candidatesToExtract =
                    await FindRecipesToExportAsync(db, innerCt);

                if (candidatesToExtract.Count < 1)
                {
                    return false;
                }

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
                    return true;
                }

                List<RecipeRecordDataExtractedFromDatabase> recordData =
                    await GetRecipeDataForExportAsync(
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
                    await ExportRecipesInSerialAsync(
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
                    await MarkRecipeExportAsSuccessNoThrowAsync(
                        db,
                        claimTag,
                        singleRecord.RecipeFk,
                        singleRecord.SearchVersion,
                        innerCt
                    );
                }

                return true;
            },
            (success, time) =>
            {
                metrics.ObserveOperation("extract_and_export_recipes", time, success);
            },
            logger.Error_UnhandledExceptionWhenSearchExporting,
            outerCt
        );

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

    private async Task MarkRecipeExportAsSuccessNoThrowAsync(
        ApplicationDbContext db,
        string claimTag,
        long recipeFk,
        long searchVersion,
        CancellationToken ct
    )
    {
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
        }
    }

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

    record class RecipeRecordDataExtractedFromDatabase(
        long RecipeFk,
        string Name,
        string ShortDescription,
        long RecipeBookFk,
        long SearchVersion
    );

    private async Task MarkRecipeExportAsFailedNoThrowAsync(
        ApplicationDbContext db,
        long recipeFk,
        long searchVersion,
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
        }
    }

    private Task<bool> PopulateEmptyRowsAsync(CancellationToken outerCt)
    {
        return repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            async (db, innerCt) =>
            {
                var entries = await db
                    .Recipes.AsNoTracking()
                    .DeleteFieldNull()
                    .Where(r =>
                        r.RecipeSearchExtraction == null
                        && r.RecipeBook!.Deleted == null
                        && r.RecipeBook!.Owner!.Deleted == null
                    )
                    .Select(r => r.Id)
                    .Take(20)
                    .ToListAsync(innerCt);
                if (entries.Count < 1)
                {
                    return false;
                }

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

                await db.SaveChangesAsync(innerCt);
                return true;
            },
            null,
            logger.Error_RunningSearchEntryCreation,
            outerCt
        );
    }

    #region Deletion
    private Task<bool> DeleteEmptyRowsAsync(CancellationToken outerCt) =>
        repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            async (db, innerCt) =>
            {
                List<long> ids = await db
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
                    .Take(20)
                    .ToListAsync(innerCt);

                if (ids.Count < 1)
                {
                    return false;
                }

                await db
                    .RecipeSearchExtractionStatusEntries.Where(e =>
                        ids.Contains(e.RecipeFk)
                        && !e.ExtractionAttempted
                        && e.LeaseExpireTime == null
                    )
                    .ExecuteDeleteAsync(innerCt);
                return true;
            },
            null,
            logger.Error_UnhandledExceptionDeletingEmptySearchRecords,
            outerCt
        );

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
                    logger.Error_DeleteBatchFailed(ex);
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
                            logger.Error_DeleteRecipeFromSearchIndexFailed(RecipeFk, innerEx);
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
            null,
            logger.Error_DeleteFromSearchIndex,
            outerCt
        );

    private static ValueTask<
        List<(long RecipeFk, string ConcurrencyTag)>
    > FindRecipeCandidatesToDelete(ApplicationDbContext db, long now, CancellationToken innerCt) =>
        db
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
            logger.Error_FailedToMarkDeleteAsFailed(recipeFk, ex);
        }
    }

    private async Task DeleteSearchRecipeSearchRecordNoThrowAsync(
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
            logger.Error_UnhandledExceptionDeletingRecipeSearchRecord(recipeFk, ex);
        }
    }

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

    private Task<bool> BreakLeaseEntriesAsync(CancellationToken outerCt)
    {
        return repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            async (db, innerCt) =>
            {
                long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
                var entries = await db
                    .RecipeSearchExtractionStatusEntries.Where(e =>
                        e.LeaseExpireTime != null
                        && (e.LeaseExpireTime < now || e.LeaseExpireTime > (now + LeaseTime))
                    )
                    .Select(e => new { e.RecipeFk, e.ConcurrencyTag })
                    .Take(20)
                    .ToListAsync(innerCt);

                if (entries.Count < 1)
                {
                    return false;
                }

                foreach (var entry in entries)
                {
                    string claimTag = concurrencyTagProvider.NextTag();
                    await db
                        .RecipeSearchExtractionStatusEntries.Where(e =>
                            e.RecipeFk == entry.RecipeFk && e.ConcurrencyTag == entry.ConcurrencyTag
                        )
                        .ExecuteUpdateAsync(
                            s =>
                                s.SetProperty(e => e.LeaseExpireTime, (long?)null)
                                    .SetProperty(e => e.ConcurrencyTag, claimTag),
                            innerCt
                        );
                }

                return true;
            },
            null,
            logger.Error_UnhandledExceptionBreakingLeaseEntries,
            outerCt
        );
    }

    private async Task<bool> DeleteStuckEntriesAsync(CancellationToken outerCt)
    {
        return await repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            async (db, innerCt) =>
            {
                var entries = await db
                    .RecipeSearchExtractionStatusEntries.Where(e =>
                        e.LeaseExpireTime == null && e.DeleteRetryCounter >= MaxBatchRetries
                    )
                    .Select(e => e.RecipeFk)
                    .Take(20)
                    .ToListAsync(innerCt);

                if (entries.Count < 1)
                {
                    return false;
                }

                await db
                    .RecipeSearchExtractionStatusEntries.Where(e =>
                        entries.Contains(e.RecipeFk)
                        && e.DeleteRetryCounter >= MaxBatchRetries
                        && e.LeaseExpireTime == null
                    )
                    .ExecuteDeleteAsync(innerCt);

                return true;
            },
            null,
            logger.Error_UnhandledExceptionDeletingStuckEntries,
            outerCt
        );
    }
}
