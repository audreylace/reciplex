using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    ILogger<RecipeSearchLeaseBreakerHostedService> logger
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable)
        {
            return;
        }

        PeriodicTimer periodicTimer = new(TimeSpan.FromSeconds(15));
        const int TenMinutesInSeconds = 10 * 60;
        while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                while (
                    await recipeSearchExportStatusRepository.BreakLeasesAsync(
                        options.Value.LeaseBreakBatchSize,
                        TenMinutesInSeconds,
                        TenMinutesInSeconds,
                        stoppingToken
                    ) > 0
                ) { }
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                logger.Error_LeaseBreakingFailed(ex);
            }
        }
    }
}
