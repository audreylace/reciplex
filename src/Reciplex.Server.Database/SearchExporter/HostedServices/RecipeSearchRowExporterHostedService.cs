using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter.HostedServices;

class RecipeSearchRowExporterHostedService(
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository,
    ISearchIndexRepository searchIndexRepository,
    IOptions<SearchExporterOptions> options,
    IConcurrencyTagProvider tagProvider,
    ILogger<RecipeSearchRowExporterHostedService> logger,
    IRecipeMutationNotifyService recipeMutationNotifyService
) : BackgroundService
{
    readonly int BatchSize = options.Value.SearchExportBatchSize;
    const int MaxRetries = 20;
    const int LeaseExpireTimeSeconds = 60 * 5; // 5 minutes

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable)
        {
            return;
        }

        var newRecipeRecovery = NewRecipesRecoveryLoopAsync(stoppingToken);
        var changedRecipeRecovery = ChangedRecipesRecoveryLoopAsync(stoppingToken);

        await Task.WhenAll(newRecipeRecovery, changedRecipeRecovery);
    }

    private async Task NewRecipesRecoveryLoopAsync(CancellationToken ct)
    {
        int counter = 0;
        while (!ct.IsCancellationRequested)
        {
            bool skipBackoff = false;
            try
            {
                string leaseToken = tagProvider.NextTag();
                var list = await recipeSearchExportStatusRepository.CreateSearchStatusRowsAsync(
                    BatchSize,
                    leaseToken,
                    LeaseExpireTimeSeconds,
                    ct
                );

                if (list.Count > 0)
                {
                    skipBackoff = await ExportRecipesAsync(list, leaseToken, ct);
                }
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                logger.Error_NewRecipeExportLoopFailed(ex);
            }
            if (skipBackoff)
            {
                counter = 0;
                recipeMutationNotifyService.DrainUpToNew(BatchSize);
                if (options.Value.SearchExportSuccessPauseMs > 0)
                {
                    await Task.Delay(options.Value.SearchExportSuccessPauseMs, ct);
                }
            }
            else
            {
                counter = Math.Min(counter + 1, 18);
                using CancellationTokenSource cancellationTokenSource =
                    CancellationTokenSource.CreateLinkedTokenSource(ct);
                cancellationTokenSource.CancelAfter(
                    TimeSpan.FromMilliseconds(20 * Math.Pow(2, counter))
                );
                try
                {
                    await recipeMutationNotifyService.WaitForNew(cancellationTokenSource.Token);
                    recipeMutationNotifyService.DrainUpToNew(BatchSize);
                    counter = 0;
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested) { }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.Error_NewRecipeExportLoopChannelFailed(ex);
                }
            }
        }
    }

    private async Task ChangedRecipesRecoveryLoopAsync(CancellationToken ct)
    {
        int counter = 0;
        while (!ct.IsCancellationRequested)
        {
            bool skipBackoff = false;
            try
            {
                string leaseToken = tagProvider.NextTag();
                var list = await recipeSearchExportStatusRepository.GetRecipesToExtractAsync(
                    BatchSize,
                    MaxRetries,
                    ct
                );

                if (list.Count > 0)
                {
                    var claimed = await recipeSearchExportStatusRepository.ClaimAsync(
                        list,
                        leaseToken,
                        LeaseExpireTimeSeconds,
                        ct
                    );

                    if (claimed > 0)
                    {
                        skipBackoff = await ExportRecipesAsync(list, leaseToken, ct);
                    }
                }
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException
                    || ex is OperationCanceledException && !ct.IsCancellationRequested
                )
            {
                logger.Error_ChangedRecipeExportLoopFailed(ex);
            }
            if (skipBackoff)
            {
                counter = 0;
                recipeMutationNotifyService.DrainUpToChange(BatchSize);
                await Task.Delay(20, ct);
            }
            else
            {
                counter = Math.Min(counter + 1, 18);
                using CancellationTokenSource cancellationTokenSource =
                    CancellationTokenSource.CreateLinkedTokenSource(ct);
                cancellationTokenSource.CancelAfter(
                    TimeSpan.FromMilliseconds(20 * Math.Pow(2, counter))
                );
                try
                {
                    await recipeMutationNotifyService.WaitForChange(cancellationTokenSource.Token);
                    recipeMutationNotifyService.DrainUpToChange(BatchSize);
                    counter = 0;
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested) { }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.Error_ChangedRecipeExportLoopChannelFailed(ex);
                }
            }
        }
    }

    private async Task<bool> ExportRecipesAsync(
        List<long> ids,
        string leaseToken,
        CancellationToken ct
    )
    {
        try
        {
            var data = await recipeSearchExportStatusRepository.ExtractRecipeDataAsync(
                ids,
                leaseToken,
                ct
            );
            if (data.Count < 1)
            {
                return true;
            }

            using CancellationTokenSource cancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(ct);
            var renewer = new LeaseRenewer(
                recipeSearchExportStatusRepository,
                leaseToken,
                [.. data.Select(d => d.RecipeFk)],
                5 * 60, // keep lease for 5 minutes
                1000 * 30 // renew every 30 seconds
            );
            var renewerTask = renewer.ExecuteAsync(cancellationTokenSource.Token);
            try
            {
                var result = await searchIndexRepository.UpsertRecipesAsync(
                    new()
                    {
                        Recipes =
                        [
                            .. data.Select(d => new RecipeSearchIndexInformation()
                            {
                                RecipeId = d.RecipeFk,
                                RecipeBookId = d.RecipeBookFk,
                                Name = d.Name,
                                ShortDescription = d.ShortDescription,
                            }),
                        ],
                    },
                    ct
                );

                switch (result)
                {
                    case IndexMutationOperationOutcome.Error:
                        return false;
                    case IndexMutationOperationOutcome.Success:
                        foreach (var recipe in data)
                        {
                            await recipeSearchExportStatusRepository.MarkRecipeAsExtractedAndReleaseAsync(
                                recipe.RecipeFk,
                                recipe.SearchVersion,
                                leaseToken,
                                ct
                            );
                        }
                        break;
                    case IndexMutationOperationOutcome.BatchFailed:
                        if (!await ExtractRecipesInSerial(leaseToken, data, renewerTask, ct))
                        {
                            return false;
                        }
                        break;
                    default:
                        throw new NotImplementedException();
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
                    logger.Error_RenewalLoopFailed(ex);
                }
            }
        }
        catch (Exception ex)
            when (ex is not OperationCanceledException
                || ex is OperationCanceledException && !ct.IsCancellationRequested
            )
        {
            logger.Error_SearchExportFailed(ex);
            return false;
        }

        return true;
    }

    private async Task<bool> ExtractRecipesInSerial(
        string leaseToken,
        List<RecipeRecordDataExtractedFromDatabase> data,
        Task renewerTask,
        CancellationToken ct
    )
    {
        foreach (var recipe in data)
        {
            if (renewerTask.IsCompleted)
            {
                return false;
            }

            IndexMutationOperationOutcome result = await searchIndexRepository.UpsertRecipesAsync(
                new()
                {
                    Recipes =
                    [
                        new RecipeSearchIndexInformation()
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
                    await recipeSearchExportStatusRepository.MarkRecipeAsExtractedAndReleaseAsync(
                        recipe.RecipeFk,
                        recipe.SearchVersion,
                        leaseToken,
                        ct
                    );
                    break;
                case IndexMutationOperationOutcome.Error:
                    return false;
                case IndexMutationOperationOutcome.BatchFailed:
                    await recipeSearchExportStatusRepository.MarkRecipeExtractionFailedAndReleaseAsync(
                        recipe.RecipeFk,
                        recipe.SearchVersion,
                        leaseToken,
                        ct
                    );
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        return true;
    }
}
