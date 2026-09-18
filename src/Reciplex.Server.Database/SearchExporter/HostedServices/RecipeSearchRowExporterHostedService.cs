using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter.HostedServices;

/// <summary>
/// exports recipes with changes to the search index
/// </summary>
/// <param name="exportStatusRepository">repository holding rows tracking search export status</param>
/// <param name="options">search options</param>
/// <param name="tagProvider">generates random tag</param>
/// <param name="logger">hosted service logger</param>
/// <param name="notifyService">notifies when recipes have changed</param>
/// <param name="searchExporterStrategy">strategy for exporting recipes to the search index</param>
class RecipeSearchRowExporterHostedService(
    IRecipeSearchExportStatusRepository exportStatusRepository,
    IOptions<SearchExporterOptions> options,
    IConcurrencyTagProvider tagProvider,
    ILogger<RecipeSearchRowExporterHostedService> logger,
    IRecipeMutationNotifyService notifyService,
    RecipeSearchIndexExporterStrategy searchExporterStrategy
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable) // exit if search is not enabled
        {
            return;
        }

        int leaseExpireTimeSeconds = (int)TimeSpan.FromMinutes(5).TotalSeconds;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                string leaseToken = tagProvider.NextTag();
                var recipeIds = await exportStatusRepository.GetRecipesToExtractAsync(
                    options.Value.SearchExportBatchSize,
                    options.Value.MaxExportAttempts,
                    stoppingToken
                );
                if (recipeIds.Count > 0)
                {
                    var claimCount = await exportStatusRepository.ClaimAsync(
                        recipeIds,
                        leaseToken,
                        leaseExpireTimeSeconds,
                        stoppingToken
                    );
                    if (claimCount > 0)
                    {
                        await searchExporterStrategy.ExportRecipesAsync(
                            recipeIds,
                            leaseToken,
                            stoppingToken
                        );
                    }
                }
                else
                {
                    await notifyService.WaitForChange(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return; // exit background service
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
}
