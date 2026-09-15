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
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable)
        {
            return;
        }

        int leaseExpireTimeSeconds = (int)TimeSpan.FromMinutes(5).TotalSeconds;
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
                        leaseExpireTimeSeconds,
                        stoppingToken
                    );

                    if (list.Count < 1)
                    {
                        break;
                    }

                    await recipeSearchIndexExporterStrategy.ExportRecipesAsync(
                        list,
                        leaseToken,
                        stoppingToken
                    );
                }
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !stoppingToken.IsCancellationRequested
                )
            {
                logger.Error_NewRecipeExportLoopFailed(ex);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            try
            {
                await recipeMutationNotifyService.WaitForNew(
                    TimeSpan.FromSeconds(30),
                    stoppingToken
                );
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested) { }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.Error_NewRecipeExportLoopChannelFailed(ex);
            }
        }
    }
}
