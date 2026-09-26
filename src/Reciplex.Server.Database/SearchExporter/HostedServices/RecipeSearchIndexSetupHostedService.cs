using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.Loggers;

namespace Reciplex.Server.Database.SearchExporter.HostedServices;

/// <summary>
/// Runs search operations in the background
/// </summary>
/// <param name="taskFactory">factory for creating search index exporter scopes</param>
/// <param name="logger">Logger for this instance</param>
sealed class RecipeSearchHostedBackgroundService(
    ISearchIndexExporterScopeFactory taskFactory,
    ILogger<RecipeSearchHostedBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await taskFactory.CreateAndExecuteOneAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return; // shutdown of host
            }
            catch (Exception ex)
            {
                logger.Error_SearchIndexExportThrew(ex);
            }

            await SafeDelay.DelayAsync(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
