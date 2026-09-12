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
    ILogger<RecipeSearchIndexDeletionHostedService> logger
) : BackgroundService
{
    readonly int MaxErrorRetries = options.Value.MaxRecipeDeleteAttempts;
    readonly int LeaseTime = 5 * 60;
    const int LeaseRenewRate = 1000 * 30;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable)
        {
            return;
        }

        PeriodicTimer periodicTimer = new(TimeSpan.FromMinutes(1));
        while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            while (
                !stoppingToken.IsCancellationRequested
                && await DeleteRecipesFromIndexAsync(stoppingToken)
            ) { }
        }
    }

    private async Task<bool> DeleteRecipesFromIndexAsync(CancellationToken ct)
    {
        try
        {
            List<long> entries = await recipeSearchExportStatusRepository.GetRecipesToDeleteAsync(
                options.Value.SearchDeleteBatchSize,
                MaxErrorRetries,
                ct
            );

            if (entries.Count < 1)
            {
                return false;
            }

            string leaseToken = concurrencyTagProvider.NextTag();
            if (
                await recipeSearchExportStatusRepository.ClaimAsync(
                    entries,
                    leaseToken,
                    LeaseTime,
                    ct
                ) < 1
            )
            {
                return true;
            }

            entries = await recipeSearchExportStatusRepository.GetClaimedRecipes(leaseToken, ct);
            if (entries.Count < 1)
            {
                return true;
            }

            LeaseRenewer renewer = new(
                recipeSearchExportStatusRepository,
                leaseToken,
                entries,
                LeaseTime,
                LeaseRenewRate
            );

            using CancellationTokenSource cancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(ct);
            var renewerTask = renewer.ExecuteAsync(cancellationTokenSource.Token);
            try
            {
                var outcome = await searchIndexRepository.DeleteRecipesAsync(entries, ct);
                switch (outcome)
                {
                    case IndexMutationOperationOutcome.Success:
                        await recipeSearchExportStatusRepository.DeleteRecipeSearchEntries(
                            entries,
                            leaseToken,
                            ct
                        );
                        return true;
                    case IndexMutationOperationOutcome.Error:
                        return false;
                    case IndexMutationOperationOutcome.BatchFailed:
                        return await DeleteRecipesFromIndexInSerialAsync(
                            entries,
                            leaseToken,
                            renewerTask,
                            ct
                        );
                }
            }
            finally
            {
                await cancellationTokenSource.CancelAsync();
                try
                {
                    await renewerTask;
                }
                catch (Exception ex)
                {
                    logger.Error_RenewTaskFailed(ex);
                }
            }
        }
        catch (Exception ex)
            when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.Error_DeleteRecipesFromIndexFailed(ex);
        }
        return false;
    }

    private async Task<bool> DeleteRecipesFromIndexInSerialAsync(
        List<long> recipeIds,
        string leaseToken,
        Task renewerTask,
        CancellationToken ct
    )
    {
        bool resetBackoff = false;
        foreach (long recipeId in recipeIds)
        {
            if (renewerTask.IsCompleted)
            {
                return false;
            }

            try
            {
                var outcome = await searchIndexRepository.DeleteRecipesAsync([recipeId], ct);
                switch (outcome)
                {
                    case IndexMutationOperationOutcome.Error:
                        break;
                    case IndexMutationOperationOutcome.BatchFailed:
                        await recipeSearchExportStatusRepository.MarkRecipeDeletionFailedAndReleaseAsync(
                            recipeId,
                            leaseToken,
                            ct
                        );
                        break;
                    case IndexMutationOperationOutcome.Success:
                        resetBackoff = true;
                        await recipeSearchExportStatusRepository.DeleteRecipeSearchEntries(
                            [recipeId],
                            leaseToken,
                            ct
                        );
                        break;
                }
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                logger.Error_DeletingRecipeFromIndexFailed(recipeId, ex);
                await Task.Delay(20, ct); // delay 20ms on error in case transient
            }
        }

        return resetBackoff;
    }
}
