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
    /// <summary>
    /// Lease lifetime before expire
    /// </summary>
    readonly int LeaseTimeSeconds = (int)TimeSpan.FromMinutes(5).TotalSeconds;

    /// <summary>
    /// Lease renewal rate
    /// </summary>
    readonly int LeaseRenewRateMs = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;

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
                if (!await DeleteRecipesFromIndexAsync(stoppingToken))
                {
                    await periodicTimer.WaitForNextTickAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return; // exit on shutdown
            }
            catch (Exception ex)
            {
                logger.Error_DeleteRecipesFromIndexFailed(ex);
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return; // exit background service
                }
                catch (Exception innerEx)
                {
                    logger.Error_ExceptionDuringPause(innerEx);
                }
            }
        }
    }

    private async Task<bool> DeleteRecipesFromIndexAsync(CancellationToken ct)
    {
        List<long> entries = await recipeSearchExportStatusRepository.GetRecipesToDeleteAsync(
            options.Value.SearchDeleteBatchSize,
            options.Value.MaxRecipeDeleteAttempts,
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
                LeaseTimeSeconds,
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

        using CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(ct);
        var renewerTask = leaseRenewer.ExecuteAsync(
            leaseToken,
            entries,
            LeaseTimeSeconds,
            LeaseRenewRateMs,
            cancellationTokenSource.Token
        );
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

        return true;
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
                IndexMutationOperationOutcome outcome =
                    await searchIndexRepository.DeleteRecipesAsync([recipeId], ct);

                switch (outcome)
                {
                    case IndexMutationOperationOutcome.Error:
                        return resetBackoff;
                    case IndexMutationOperationOutcome.BatchFailed:
                        await recipeSearchExportStatusRepository.MarkRecipeDeletionFailedAndReleaseAsync(
                            recipeId,
                            leaseToken,
                            ct
                        );
                        break;
                    case IndexMutationOperationOutcome.Success:
                        await recipeSearchExportStatusRepository.DeleteRecipeSearchEntries(
                            [recipeId],
                            leaseToken,
                            ct
                        );
                        resetBackoff = true;
                        break;
                }
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                logger.Error_DeletingRecipeFromIndexFailed(recipeId, ex);
                await Task.Delay(TimeSpan.FromMilliseconds(50), ct);
            }
        }

        return resetBackoff;
    }
}
