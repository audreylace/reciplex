using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter.HostedServices;

/// <summary>
/// Runs a periodic loop breaking leases that have not been renewed in the required time window
/// </summary>
/// <param name="repo">repository holding database rows storing search export state</param>
/// <param name="options">search options</param>
/// <param name="logger">logger for the hosted service</param>
/// <param name="resilienceFactory">factory for building a resilience pipeline</param>
sealed class RecipeSearchLeaseBreakerHostedService(
    IRecipeSearchExportStatusRepository repo,
    IOptions<SearchExporterOptions> options,
    ILogger<RecipeSearchLeaseBreakerHostedService> logger,
    ResiliencePipelineBuilderFactory resilienceFactory
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable) // exit if search is not enabled
        {
            return;
        }

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

    private async Task BreakLeaseLoopAsync(
        ResiliencePipeline pipeline,
        CancellationToken stoppingToken
    )
    {
        int TenMinutesInSeconds = (int)TimeSpan.FromMinutes(10).TotalSeconds;
        while (
            !stoppingToken.IsCancellationRequested
            && await pipeline.ExecuteAsync(
                async token =>
                    await repo.BreakLeasesAsync(
                        options.Value.LeaseBreakBatchSize,
                        TenMinutesInSeconds,
                        TenMinutesInSeconds,
                        token
                    ),
                stoppingToken
            ) > 0
        ) { }
    }
}
