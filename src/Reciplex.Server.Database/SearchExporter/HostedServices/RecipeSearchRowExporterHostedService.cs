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
                    var recipeIds =
                        await recipeSearchExportStatusRepository.GetRecipesToExtractAsync(
                            options.Value.SearchExportBatchSize,
                            options.Value.MaxExportAttempts,
                            stoppingToken
                        );
                    if (recipeIds.Count < 1)
                    {
                        break;
                    }

                    var claimCount = await recipeSearchExportStatusRepository.ClaimAsync(
                        recipeIds,
                        leaseToken,
                        leaseExpireTimeSeconds,
                        stoppingToken
                    );
                    if (claimCount > 0)
                    {
                        await recipeSearchIndexExporterStrategy.ExportRecipesAsync(
                            recipeIds,
                            leaseToken,
                            stoppingToken
                        );
                    }
                }
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !stoppingToken.IsCancellationRequested
                )
            {
                logger.Error_ChangedRecipeExportLoopFailed(ex);

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException) { }
            }

            try
            {
                await recipeMutationNotifyService.WaitForChange(
                    TimeSpan.FromSeconds(30),
                    stoppingToken
                );
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested) { }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.Error_ChangedRecipeExportLoopChannelFailed(ex);
            }
        }
    }
}
