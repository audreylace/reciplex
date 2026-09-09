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
    readonly int MaxErrorRetries = 20;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable)
        {
            return;
        }

        int backOffCounter = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            bool purgedBrokenRecords = await PurgeStuckRecipesAsync(stoppingToken);
            bool deletedRecordsFromIndex = await DeleteRecipesFromIndexAsync(stoppingToken);
            if (purgedBrokenRecords || deletedRecordsFromIndex)
            {
                backOffCounter = 0;
                if (options.Value.SearchExportDeleteLoopPauseMs > 0)
                {
                    await Task.Delay(options.Value.SearchExportDeleteLoopPauseMs, stoppingToken);
                }
            }
            else
            {
                backOffCounter = Math.Min(backOffCounter + 1, 12);
                await Task.Delay(TimeSpan.FromSeconds(backOffCounter), stoppingToken);
            }
        }
    }

    private async Task<bool> DeleteRecipesFromIndexAsync(CancellationToken ct)
    {
        bool didWork = false;
        try
        {
            while (!ct.IsCancellationRequested)
            {
                List<long> entries =
                    await recipeSearchExportStatusRepository.GetRecipesToDeleteAsync(
                        options.Value.SearchDeleteBatchSize,
                        MaxErrorRetries,
                        ct
                    );
                if (entries.Count < 1)
                {
                    break;
                }
                string leaseToken = concurrencyTagProvider.NextTag();

                if (
                    await recipeSearchExportStatusRepository.ClaimAsync(
                        entries,
                        leaseToken,
                        5 * 60,
                        ct
                    ) > 0
                )
                {
                    entries = await recipeSearchExportStatusRepository.GetClaimedRecipes(
                        leaseToken,
                        ct
                    );

                    if (entries.Count > 0)
                    {
                        var renewer = new LeaseRenewer(
                            recipeSearchExportStatusRepository,
                            leaseToken,
                            entries,
                            5 * 60, // keep lease for 5 minutes
                            1000 * 30 // renew every 30 seconds
                        );

                        using CancellationTokenSource cancellationTokenSource =
                            CancellationTokenSource.CreateLinkedTokenSource(ct);
                        var renewerTask = renewer.ExecuteAsync(cancellationTokenSource.Token);

                        try
                        {
                            var outcome = await searchIndexRepository.DeleteRecipesAsync(
                                entries,
                                ct
                            );
                            switch (outcome)
                            {
                                case IndexMutationOperationOutcome.Success:
                                    didWork = true;
                                    await recipeSearchExportStatusRepository.DeleteRecipeSearchEntries(
                                        entries,
                                        leaseToken,
                                        ct
                                    );
                                    break;
                                case IndexMutationOperationOutcome.Error:
                                    return didWork;
                                case IndexMutationOperationOutcome.BatchFailed:
                                    didWork |= await DeleteRecipesFromIndexInSerialAsync(
                                        entries,
                                        leaseToken,
                                        renewerTask,
                                        ct
                                    );
                                    break;
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
                }

                if (options.Value.SearchExportDeleteLoopPauseMs > 0)
                {
                    await Task.Delay(options.Value.SearchExportDeleteLoopPauseMs, ct);
                }
            }
        }
        catch (Exception ex)
            when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.Error_DeleteRecipesFromIndexFailed(ex);
        }

        return didWork;
    }

    private async Task<bool> PurgeStuckRecipesAsync(CancellationToken ct)
    {
        bool anyWork = false;
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var entries =
                    await recipeSearchExportStatusRepository.PurgeRecipeSearchEntriesWithTooManyRetries(
                        100,
                        MaxErrorRetries,
                        ct
                    );
                if (entries < 1)
                {
                    break;
                }
                anyWork = true;
                if (options.Value.SearchExportDeleteLoopPauseMs > 0)
                {
                    await Task.Delay(options.Value.SearchExportDeleteLoopPauseMs, ct);
                }
            }
        }
        catch (Exception ex)
            when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.Error_PurgingRecipesFailed(ex);
        }

        return anyWork;
    }

    private async Task<bool> DeleteRecipesFromIndexInSerialAsync(
        List<long> entries,
        string leaseToken,
        Task renewerTask,
        CancellationToken ct
    )
    {
        bool didWork = false;
        foreach (var entry in entries)
        {
            if (renewerTask.IsCompleted)
            {
                return didWork;
            }

            var outcome = await searchIndexRepository.DeleteRecipesAsync([entry], ct);
            if (outcome == IndexMutationOperationOutcome.Error)
            {
                return didWork;
            }
            else if (outcome == IndexMutationOperationOutcome.BatchFailed)
            {
                await recipeSearchExportStatusRepository.MarkRecipeDeletionFailedAndReleaseAsync(
                    entry,
                    leaseToken,
                    ct
                );
            }
            else if (outcome == IndexMutationOperationOutcome.Success)
            {
                didWork = true;
                await recipeSearchExportStatusRepository.DeleteRecipeSearchEntries(
                    [entry],
                    leaseToken,
                    ct
                );
            }
        }

        return didWork;
    }
}
