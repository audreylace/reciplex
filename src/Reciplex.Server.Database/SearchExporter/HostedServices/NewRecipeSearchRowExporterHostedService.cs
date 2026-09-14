using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter.HostedServices;

class NewRecipeSearchRowExporterHostedService(
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository,
    IConcurrencyTagProvider tagProvider,
    IRecipeMutationNotifyService recipeMutationNotifyService,
    RecipeSearchIndexExporterStrategy recipeSearchIndexExporterStrategy,
    IOptions<SearchExporterOptions> options,
    ILogger<NewRecipeSearchRowExporterHostedService> logger
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
                    var list = await recipeSearchExportStatusRepository.CreateSearchStatusRowsAsync(
                        options.Value.SearchExportBatchSize,
                        leaseToken,
                        LeaseExpireTimeSeconds,
                        stoppingToken
                    );

                    if (list.Count < 1)
                    {
                        break;
                    }

                    if (
                        await recipeSearchIndexExporterStrategy.ExportRecipesAsync(
                            list,
                            leaseToken,
                            stoppingToken
                        )
                    )
                    {
                        recipeMutationNotifyService.DrainUpToNew(list.Count);
                    }
                }
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !stoppingToken.IsCancellationRequested
                )
            {
                logger.Error_NewRecipeExportLoopFailed(ex);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            using CancellationTokenSource cancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
            try
            {
                await recipeMutationNotifyService.WaitForNew(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested) { }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.Error_NewRecipeExportLoopChannelFailed(ex);
            }
        }
    }
}
