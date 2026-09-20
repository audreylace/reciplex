using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter.SearchBackgroundTasks;

/// <summary>
/// Runs a periodic loop breaking leases that have not been renewed in the required time window
/// </summary>
/// <param name="repo">repository holding database rows storing search export state</param>
/// <param name="options">search options</param>
/// <param name="logger">logger for the hosted service</param>
/// <param name="resilienceFactory">factory for building a resilience pipeline</param>
sealed class RecipeLeaseBreakerSearchBackgroundTask(
    IRecipeSearchExportStatusRepository repo,
    IOptions<SearchExporterOptions> options,
    ILogger<RecipeLeaseBreakerSearchBackgroundTask> logger,
    ResiliencePipelineBuilderFactory resilienceFactory
) : ISearchBackgroundTask
{
    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ResiliencePipeline pipeline = resilienceFactory.BuildDeleteRowPipeline(ex =>
            ex is not OperationCanceledException || !stoppingToken.IsCancellationRequested
        );
        using PeriodicTimer periodicTimer = new(TimeSpan.FromMinutes(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await BreakLeaseLoopAsync(pipeline, stoppingToken);
                if (!await periodicTimer.WaitForNextTickAsync(stoppingToken))
                {
                    return; // exit on shutdown
                }
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

    private async Task BreakLeaseLoopAsync(
        ResiliencePipeline pipeline,
        CancellationToken stoppingToken
    )
    {
        while (
            !stoppingToken.IsCancellationRequested
            && await pipeline.ExecuteAsync(
                async token =>
                    await repo.BreakLeasesAsync(
                        options.Value.LeaseBreakBatchSize,
                        TimeSpan.FromMinutes(10),
                        TimeSpan.FromMinutes(10),
                        token
                    ),
                stoppingToken
            ) > 0
        ) { }
    }
}
