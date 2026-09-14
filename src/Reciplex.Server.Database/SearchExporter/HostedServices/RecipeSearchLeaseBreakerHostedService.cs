using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter.HostedServices;

/// <summary>
/// Runs a periodic loop breaking stuck leases
/// </summary>
/// <param name="sp">service provider for opening scopes</param>
/// <param name="options">search exporter options to know if search feature is enabled</param>
/// <param name="logger">The service logger</param>
sealed class RecipeSearchLeaseBreakerHostedService(
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository,
    IOptions<SearchExporterOptions> options,
    ILogger<RecipeSearchLeaseBreakerHostedService> logger,
    ResiliencePipelineBuilderFactory resiliencePipelineBuilderFactory
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable)
        {
            return;
        }

        ResiliencePipeline pipeline = resiliencePipelineBuilderFactory.BuildDeleteRowPipeline(ex =>
            ex is not OperationCanceledException || !stoppingToken.IsCancellationRequested
        );
        PeriodicTimer periodicTimer = new(TimeSpan.FromMinutes(1));
        const int TenMinutesInSeconds = 10 * 60;
        while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await BreakLeaseLoopAsync(pipeline, TenMinutesInSeconds, stoppingToken);
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !stoppingToken.IsCancellationRequested
                )
            {
                logger.Error_LeaseBreakingFailed(ex);
            }
        }
    }

    private async Task BreakLeaseLoopAsync(
        ResiliencePipeline pipeline,
        int TenMinutesInSeconds,
        CancellationToken stoppingToken
    )
    {
        while (
            !stoppingToken.IsCancellationRequested
            && await pipeline.ExecuteAsync(
                async token =>
                    await recipeSearchExportStatusRepository.BreakLeasesAsync(
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
