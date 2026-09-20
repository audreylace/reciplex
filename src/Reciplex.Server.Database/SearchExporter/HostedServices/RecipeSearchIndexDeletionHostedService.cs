using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter.HostedServices;

/// <summary>
/// Purges recipes from the search index periodically
/// </summary>
sealed class RecipeSearchIndexDeletionHostedService(
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository,
    ISearchIndexRepository searchIndexRepository,
    IOptions<SearchExporterOptions> options,
    IConcurrencyTagProvider concurrencyTagProvider,
    LeaseRenewer leaseRenewer,
    ILogger<RecipeSearchIndexDeletionHostedService> logger
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable) // exit if search is not enabled
        {
            return;
        }

        using PeriodicTimer periodicTimer = new(TimeSpan.FromMinutes(1));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // look for potential recipes that could be deleted from the search index
                List<long> entries =
                    await recipeSearchExportStatusRepository.GetRecipesToDeleteAsync(
                        options.Value.SearchDeleteBatchSize,
                        options.Value.MaxRecipeDeleteAttempts,
                        stoppingToken
                    );

                // delay if no recipes were found, otherwise delete them
                if (entries.Count < 1)
                {
                    if (!await periodicTimer.WaitForNextTickAsync(stoppingToken))
                    {
                        return; // exit on shutdown; periodic timer returns false when `stoppingToken` is cancelled.
                    }
                }
                else
                {
                    await DeleteRecipesFromIndexAsync(entries, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return; // exit on shutdown
            }
            catch (Exception ex)
            {
                logger.Error_DeleteRecipesFromIndexFailed(ex);

                // delay to prevent a tight loop on repeated failure
                await SafeDelay.DelayAsync(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Attempts to claim some subset of <paramref name="candidates"/> and then delete them from the search index
    /// </summary>
    /// <param name="candidates">ids to claim and delete</param>
    /// <param name="ct">cancels the operation</param>
    private async Task DeleteRecipesFromIndexAsync(List<long> candidates, CancellationToken ct)
    {
        // generate a unique lease token to identify our lease
        string leaseToken = concurrencyTagProvider.NextTag();

        if (
            await recipeSearchExportStatusRepository.ClaimAsync(
                candidates,
                leaseToken,
                TimeSpan.FromMinutes(5),
                ct
            ) < 1
        )
        {
            return; // claimed nothing; exit
        }

        // determine the actual ids of claimed rows as its not guaranteed that we got all of candidates
        var claimedIds = await recipeSearchExportStatusRepository.GetClaimedRecipes(leaseToken, ct);
        if (claimedIds.Count < 1)
        {
            return;
        }

        using CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(ct);

        // renew held rows in a background task
        Task renewerTask = RenewIdsPeriodicallyAsync(
            leaseToken,
            claimedIds,
            cancellationTokenSource
        );

        bool delay = false;

        // Ensure unlock on exception to minimize delays.
        // Otherwise rows will be locked up to 10 minutes
        // resulting in end user delays.
        List<long> recipesToUnlock = claimedIds;

        List<long> recipesToDelete = [];
        List<long> recipesThatFailed = [];
        try
        {
            var outcome = await searchIndexRepository.DeleteRecipesAsync(
                claimedIds,
                cancellationTokenSource.Token
            );
            switch (outcome)
            {
                case IndexMutationOperationOutcome.Success:
                    recipesToUnlock = [];
                    recipesToDelete = claimedIds; // delete will unlock for us
                    break;

                case IndexMutationOperationOutcome.Error:
                    delay = true; // pause to avoid tight loop during search index outage
                    break;

                case IndexMutationOperationOutcome.BatchFailed:
                    // Try one by one in case the failure was caused by one or several in the batch.
                    // The search index aborts the batch upload when one or more failed.
                    (recipesToDelete, recipesThatFailed, recipesToUnlock) =
                        await DeleteRecipesFromIndexInSerialAsync(
                            claimedIds,
                            cancellationTokenSource.Token
                        );
                    break;
            }
        }
        finally
        {
            await cancellationTokenSource.CancelAsync();
            await renewerTask;
        }

        if (ct.IsCancellationRequested)
        {
            return;
        }

        // records outcomes to the database and clear leases
        await UpdateSearchRowsAsync(
            leaseToken,
            recipesToUnlock,
            recipesToDelete,
            recipesThatFailed,
            ct
        );

        if (delay)
        {
            await SafeDelay.DelayAsync(TimeSpan.FromSeconds(5), ct);
        }
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
        {
            return;
        }
        catch (Exception ex)
        {
            logger.Error_RenewTaskFailed(ex);
        }

        if (!cancellationTokenSource.IsCancellationRequested)
        {
            await cancellationTokenSource.CancelAsync();
        }
    }

    /// <summary>
    /// Releases leases and records export outcomes to database.
    /// <paramref name="recipesThatFailed"/>, <paramref name="recipesToDelete"/>, <paramref name="recipesToUnlock"/>
    /// should not have any overlapping ids.
    /// </summary>
    /// <param name="leaseToken">token identifying the lease</param>
    /// <param name="recipesToUnlock">recipes whose lease should be cleared and no outcome recorded</param>
    /// <param name="recipesToDelete">recipes dropped from the search index and whose rows should be dropped from the database</param>
    /// <param name="recipesThatFailed">recipes whose deletion failed</param>
    /// <param name="ct">cancels the async operation</param>
    private async Task UpdateSearchRowsAsync(
        string leaseToken,
        List<long> recipesToUnlock,
        List<long> recipesToDelete,
        List<long> recipesThatFailed,
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
                logger.Error_FailedToClearLeases(ex);
            }
        }

        if (recipesToDelete.Count > 0)
        {
            try
            {
                await recipeSearchExportStatusRepository.DeleteRecipeSearchEntries(
                    recipesToDelete,
                    leaseToken,
                    ct
                );
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                logger.Error_FailedToDeleteRows(ex);
            }
        }

        if (recipesThatFailed.Count > 0)
        {
            try
            {
                await recipeSearchExportStatusRepository.MarkRecipesDeletionFailedAndReleaseAsync(
                    recipesThatFailed,
                    leaseToken,
                    ct
                );
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                logger.Error_FailedToIncrementRowsFailureCounter(ex);
            }
        }
    }

    /// <summary>
    /// Deletes recipes from the search index one by one recording the outcome of each
    /// </summary>
    /// <param name="recipeIds">set of ids</param>
    /// <param name="ct">cancels the async operation</param>
    /// <returns>
    /// tuple: first is the recipes dropped from the index, second set of recipes whose drop outcome failed,
    /// third is the set of recipes skipped because <paramref name="ct"/> was cancelled or
    /// because of a network error.
    /// </returns>
    private async Task<(
        List<long> RecipesToDelete,
        List<long> RecipesThatFailed,
        List<long> RecipesToUnlock
    )> DeleteRecipesFromIndexInSerialAsync(List<long> recipeIds, CancellationToken ct)
    {
        List<long> recipesToDelete = [];
        List<long> recipesThatFailed = [];
        HashSet<long> recipesHandled = [.. recipeIds];

        foreach (long recipeId in recipeIds)
        {
            if (ct.IsCancellationRequested)
            {
                return new(recipesToDelete, recipesThatFailed, [.. recipesHandled]);
            }

            try
            {
                switch (await searchIndexRepository.DeleteRecipesAsync([recipeId], ct))
                {
                    case IndexMutationOperationOutcome.BatchFailed:
                        recipesThatFailed.Add(recipeId);
                        recipesHandled.Remove(recipeId);
                        logger.Error_DeletingRecipeFromIndexFailed(recipeId);
                        break;
                    case IndexMutationOperationOutcome.Success:
                        recipesToDelete.Add(recipeId);
                        recipesHandled.Remove(recipeId);
                        break;
                    default:
                        return new(recipesToDelete, recipesThatFailed, [.. recipesHandled]);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                recipesThatFailed.Add(recipeId);
                recipesHandled.Remove(recipeId);
                logger.Error_DeletingRecipeFromIndexFailed(recipeId, ex);
                if (!await SafeDelay.DelayAsync(TimeSpan.FromMilliseconds(100), ct))
                {
                    break;
                }
            }
        }

        return new(recipesToDelete, recipesThatFailed, [.. recipesHandled]);
    }
}
