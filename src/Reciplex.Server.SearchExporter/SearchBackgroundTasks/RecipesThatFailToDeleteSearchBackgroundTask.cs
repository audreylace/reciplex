using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Database.SearchIndexTrackingDomain;
using Reciplex.Server.SearchExporter.Loggers;
using Reciplex.Server.SearchExporter.Options;

namespace Reciplex.Server.SearchExporter.SearchBackgroundTasks;

/// <summary>
/// Background service that purges recipes with too many
/// delete attempts.
/// </summary>
/// <param name="repository">repository managing recipe search rows</param>
/// <param name="options">options controlling the behavior of this background service</param>
/// <param name="logger">logger for the hosted service</param>
sealed class RecipesThatFailToDeleteSearchBackgroundTask(
    IRecipeSearchExportStatusRepository repository,
    IOptions<SearchExporterOptions> options,
    ILogger<RecipesThatFailToDeleteSearchBackgroundTask> logger
) : ISearchBackgroundTask
{
    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer periodicTimer = new(TimeSpan.FromMinutes(1));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await repository.PurgeRowsWithMaxDeleteAttemptsAsync(
                    options.Value.MaxRecipeDeleteAttempts,
                    stoppingToken
                );
                await periodicTimer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return; // exit on shutdown
            }
            catch (Exception ex)
            {
                logger.Error_PurgingRecipesFailed(ex);
                await SafeDelay.DelayAsync(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
