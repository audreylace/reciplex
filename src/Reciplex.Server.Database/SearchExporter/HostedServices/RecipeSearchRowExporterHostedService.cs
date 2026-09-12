using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter.HostedServices;

class RecipeSearchRowExporterHostedService(
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository,
    IOptions<SearchExporterOptions> options,
    IConcurrencyTagProvider tagProvider,
    ILogger<RecipeSearchRowExporterHostedService> logger,
    IRecipeMutationNotifyService recipeMutationNotifyService,
    RecipeSearchIndexExporterStrategy recipeSearchIndexExporterStrategy
) : BackgroundService
{
    const int LeaseExpireTimeSeconds = 60 * 5; // 5 minutes

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    string leaseToken = tagProvider.NextTag();
                    var list = await recipeSearchExportStatusRepository.GetRecipesToExtractAsync(
                        options.Value.SearchExportBatchSize,
                        options.Value.MaxExportAttempts,
                        stoppingToken
                    );
                    if (list.Count < 1)
                    {
                        break;
                    }

                    if (
                        await recipeSearchExportStatusRepository.ClaimAsync(
                            list,
                            leaseToken,
                            LeaseExpireTimeSeconds,
                            stoppingToken
                        ) > 0
                    )
                    {
                        if (
                            await recipeSearchIndexExporterStrategy.ExportRecipesAsync(
                                list,
                                leaseToken,
                                stoppingToken
                            )
                        )
                        {
                            recipeMutationNotifyService.DrainUpToChange(
                                options.Value.SearchExportBatchSize
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException
                    || ex is OperationCanceledException && !stoppingToken.IsCancellationRequested
                )
            {
                logger.Error_ChangedRecipeExportLoopFailed(ex);
            }

            using CancellationTokenSource cancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
            try
            {
                await recipeMutationNotifyService.WaitForChange(cancellationTokenSource.Token);
                recipeMutationNotifyService.DrainUpToChange(options.Value.SearchExportBatchSize);
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested) { }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.Error_ChangedRecipeExportLoopChannelFailed(ex);
            }
        }
    }
}
