using Microsoft.Extensions.Logging;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Database.SearchIndexTrackingDomain;
using Reciplex.Server.SearchExporter.Loggers;

namespace Reciplex.Server.SearchExporter.SearchBackgroundTasks;

/// <summary>
/// Runs a periodic loop breaking leases that have not been renewed in the required time window
/// </summary>
/// <param name="repo">repository holding database rows storing search export state</param>
/// <param name="logger">logger for the hosted service</param>
sealed class RecipeLeaseBreakerSearchBackgroundTask(
    IRecipeSearchExportStatusRepository repo,
    ILogger<RecipeLeaseBreakerSearchBackgroundTask> logger
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
                await repo.BreakLeasesAsync(
                    TimeSpan.FromMinutes(10),
                    TimeSpan.FromMinutes(10),
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
                logger.Error_LeaseBreakingFailed(ex);
                await SafeDelay.DelayAsync(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
