using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter;

/// <summary>
/// Strategy for exporting a set of rows to the search index
/// </summary>
/// <param name="recipeSearchExportStatusRepository">search status repository</param>
/// <param name="searchIndexRepository">repository representing the search index</param>
/// <param name="leaseRenewer">strategy for renewing leases held while extraction takes place</param>
/// <param name="logger">instance logger</param>
sealed class RecipeSearchIndexExporterStrategy(
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository,
    ISearchIndexRepository searchIndexRepository,
    LeaseRenewer leaseRenewer,
    ILogger<RecipeSearchIndexExporterStrategy> logger
)
{
    /// <summary>
    /// Exports a set of rows keyed by <paramref name="ids"/> with lease <paramref name="leaseToken"/>.
    /// On completion, database is updated and lease cleared.
    /// </summary>
    /// <param name="ids">set of row primary ids</param>
    /// <param name="leaseToken">the token of the held lease</param>
    /// <param name="maxExportAttempts">max number of export attempts</param>
    /// <param name="ct">async cancellation token</param>
    public async Task ExportRecipesAsync(
        List<long> ids,
        string leaseToken,
        int maxExportAttempts,
        CancellationToken ct
    )
    {
        var recipeData = await recipeSearchExportStatusRepository.ExtractRecipeDataAsync(
            ids,
            leaseToken,
            ct
        );
        if (recipeData.Count < 1)
        {
            return; // someone broke our leases or they expired
        }

        using CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(ct);
        var renewerTask = RenewIdsPeriodicallyAsync(
            leaseToken,
            [.. recipeData.Select(d => d.RecipeFk)],
            cancellationTokenSource
        );

        List<long> idsToUnlock = [.. recipeData.Select(d => d.RecipeFk)];
        List<RecipeRecordDataExtractedFromDatabase> idsExtracted = [];
        List<RecipeRecordDataExtractedFromDatabase> idsFailed = [];
        try
        {
            IndexMutationOperationOutcome result = await searchIndexRepository.UpsertRecipesAsync(
                new()
                {
                    Recipes =
                    [
                        .. recipeData.Select(d => new RecipeSearchIndexDocument()
                        {
                            RecipeId = d.RecipeFk,
                            RecipeBookId = d.RecipeBookFk,
                            Name = d.Name,
                            ShortDescription = d.ShortDescription,
                        }),
                    ],
                },
                cancellationTokenSource.Token
            );

            switch (result)
            {
                case IndexMutationOperationOutcome.Success:
                    idsExtracted = recipeData;
                    idsToUnlock = [];
                    break;
                case IndexMutationOperationOutcome.BatchFailed:
                    (idsToUnlock, idsExtracted, idsFailed) = await ExtractRecipesInSerial(
                        recipeData,
                        cancellationTokenSource.Token
                    );
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            await CleanupAsync(
                leaseToken,
                [.. recipeData.Select(d => d.RecipeFk)],
                cancellationTokenSource,
                renewerTask,
                maxExportAttempts,
                ct
            );
            return;
        }
        catch (Exception)
        {
            await CleanupAsync(
                leaseToken,
                [.. recipeData.Select(d => d.RecipeFk)],
                cancellationTokenSource,
                renewerTask,
                maxExportAttempts,
                ct
            );
            throw;
        }

        await UpdateSearchRowsAsync(
            leaseToken,
            idsToUnlock,
            idsExtracted,
            idsFailed,
            maxExportAttempts,
            ct
        );
    }

    private async Task CleanupAsync(
        string leaseToken,
        List<long> ids,
        CancellationTokenSource cancellationTokenSource,
        Task renewerTask,
        int maxExportAttempts,
        CancellationToken ct
    )
    {
        try
        {
            try
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    await cancellationTokenSource.CancelAsync();
                }
            }
            catch (ObjectDisposedException) { }

            await renewerTask;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            logger.Error_RenewTaskFailed(ex);
        }

        try
        {
            if (ct.IsCancellationRequested) // release locks on shutdown
            {
                using CancellationTokenSource cancellation = new();
                cancellation.CancelAfter(TimeSpan.FromSeconds(5));
                await UpdateSearchRowsAsync(
                    leaseToken,
                    ids,
                    [],
                    [],
                    maxExportAttempts,
                    cancellation.Token
                );
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            logger.Error_LeaseReleaseFailed(ex);
        }
    }

    private async Task<(
        List<long> IdsToUnlock,
        List<RecipeRecordDataExtractedFromDatabase> IdsExtracted,
        List<RecipeRecordDataExtractedFromDatabase> IdsFailed
    )> ExtractRecipesInSerial(
        List<RecipeRecordDataExtractedFromDatabase> data,
        CancellationToken ct
    )
    {
        HashSet<long> idsToUnlock = [.. data.Select(d => d.RecipeFk)];
        List<RecipeRecordDataExtractedFromDatabase> idsExtracted = [];
        List<RecipeRecordDataExtractedFromDatabase> idsFailed = [];

        foreach (var recipe in data)
        {
            if (ct.IsCancellationRequested)
            {
                return new([.. idsToUnlock], idsExtracted, idsFailed);
            }

            try
            {
                IndexMutationOperationOutcome result =
                    await searchIndexRepository.UpsertRecipesAsync(
                        new()
                        {
                            Recipes =
                            [
                                new RecipeSearchIndexDocument()
                                {
                                    RecipeId = recipe.RecipeFk,
                                    RecipeBookId = recipe.RecipeBookFk,
                                    Name = recipe.Name,
                                    ShortDescription = recipe.ShortDescription,
                                },
                            ],
                        },
                        ct
                    );

                switch (result)
                {
                    case IndexMutationOperationOutcome.Success:
                        idsExtracted.Add(recipe);
                        idsToUnlock.Remove(recipe.RecipeFk);
                        break;

                    case IndexMutationOperationOutcome.BatchFailed:
                        idsFailed.Add(recipe);
                        idsToUnlock.Remove(recipe.RecipeFk);
                        logger.Error_SearchExportFailed(recipe.RecipeFk);
                        break;

                    default:
                        return new([.. idsToUnlock], idsExtracted, idsFailed);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.Error_SearchExportFailed(recipe.RecipeFk, ex);
                idsFailed.Add(recipe);
                idsToUnlock.Remove(recipe.RecipeFk);
                if (!await SafeDelay.DelayAsync(TimeSpan.FromMilliseconds(100), ct))
                {
                    break;
                }
            }
        }

        return new([.. idsToUnlock], idsExtracted, idsFailed);
    }

    /// <summary>
    /// Renews leases on rows periodically until the token bound to <paramref name="cancellationTokenSource"/>
    /// is cancelled.
    /// </summary>
    /// <param name="leaseToken">token identifying our lease</param>
    /// <param name="claimedIds">the ids to renew</param>
    /// <param name="cancellationTokenSource">token cancellation source. Cancelled when the renewer task returns.</param>
    private async Task RenewIdsPeriodicallyAsync(
        string leaseToken,
        List<long> claimedIds,
        CancellationTokenSource cancellationTokenSource
    )
    {
        try
        {
            await leaseRenewer.ExecuteAsync(
                leaseToken,
                claimedIds,
                TimeSpan.FromMinutes(5), // lease is valid for 5 minutes
                TimeSpan.FromSeconds(30), // renew every 30 seconds
                8, // try renew up to 8 times on failure
                cancellationTokenSource.Token
            );
        }
        catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
        { }
        finally
        {
            try
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    await cancellationTokenSource.CancelAsync();
                }
            }
            catch (ObjectDisposedException) { }
        }
    }

    /// <summary>
    /// Releases leases and records export outcomes to database.
    /// <paramref name="recipesThatFailed"/>, <paramref name="recipesExtracted"/>, <paramref name="recipesToUnlock"/>
    /// should not have any overlapping ids.
    /// </summary>
    /// <param name="leaseToken">token identifying the lease</param>
    /// <param name="recipesToUnlock">recipes whose lease should be cleared and no outcome recorded</param>
    /// <param name="recipesExtracted">recipes dropped from the search index and whose rows should be dropped from the database</param>
    /// <param name="recipesThatFailed">recipes whose deletion failed</param>
    /// <param name="maxAttempts">max number of export attempts</param>
    /// <param name="ct">cancels the async operation</param>
    private async Task UpdateSearchRowsAsync(
        string leaseToken,
        List<long> recipesToUnlock,
        List<RecipeRecordDataExtractedFromDatabase> recipesExtracted,
        List<RecipeRecordDataExtractedFromDatabase> recipesThatFailed,
        int maxAttempts,
        CancellationToken ct
    )
    {
        if (recipesToUnlock.Count > 0)
        {
            try
            {
                await recipeSearchExportStatusRepository.ClearLeasesAsync(
                    recipesToUnlock,
                    leaseToken,
                    ct
                );
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                logger.Error_LeaseReleaseFailed(ex);
            }
        }

        if (recipesExtracted.Count > 0)
        {
            foreach (var rowPtr in recipesExtracted)
            {
                try
                {
                    await recipeSearchExportStatusRepository.MarkRecipeAsExtractedAndReleaseAsync(
                        rowPtr.RecipeFk,
                        rowPtr.SearchVersion,
                        leaseToken,
                        ct
                    );
                }
                catch (Exception ex)
                    when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
                {
                    logger.Error_MarkingAsExtractedFailed(rowPtr.RecipeFk, ex);
                }
            }
        }

        if (recipesThatFailed.Count > 0)
        {
            foreach (var rowPtr in recipesThatFailed)
            {
                try
                {
                    await recipeSearchExportStatusRepository.MarkRecipeExtractionFailedAndReleaseAsync(
                        rowPtr.RecipeFk,
                        rowPtr.SearchVersion,
                        leaseToken,
                        maxAttempts,
                        ct
                    );
                }
                catch (Exception ex)
                    when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
                {
                    logger.Error_IncrementingExtractionAttemptFailed(rowPtr.RecipeFk, ex);
                }
            }
        }
    }
}
