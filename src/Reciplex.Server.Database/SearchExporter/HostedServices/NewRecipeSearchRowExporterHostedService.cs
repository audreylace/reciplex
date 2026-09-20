using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter.HostedServices;

/// <summary>
/// Finds and exports new recipes
/// </summary>
/// <param name="searchRepo">repository for search status records in the database</param>
/// <param name="tagProvider">provides random tags</param>
/// <param name="notificationService">notifies when new recipes are created and ready for export</param>
/// <param name="exporterStrategy">strategy for marshalling records to the search index</param>
/// <param name="options">options for the service</param>
/// <param name="logger">service logger</param>
class NewRecipeSearchRowExporterHostedService(
    IRecipeSearchExportStatusRepository searchRepo,
    IConcurrencyTagProvider tagProvider,
    IRecipeMutationNotifyService notificationService,
    RecipeSearchIndexExporterStrategy exporterStrategy,
    IOptions<SearchExporterOptions> options,
    ILogger<NewRecipeSearchRowExporterHostedService> logger
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable) // exit if search is not enabled
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                string leaseToken = tagProvider.NextTag();
                var list = await searchRepo.CreateSearchStatusRowsAsync(
                    options.Value.SearchExportBatchSize,
                    leaseToken,
                    TimeSpan.FromMinutes(5),
                    stoppingToken
                );
                if (list.Count < 1)
                {
                    await notificationService.WaitForNew(TimeSpan.FromSeconds(30), stoppingToken);
                }
                else
                {
                    await exporterStrategy.ExportRecipesAsync(list, leaseToken, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return; // exit background service
            }
            catch (Exception ex)
            {
                logger.Error_NewRecipeExportLoopFailed(ex);
                await SafeDelay.DelayAsync(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
