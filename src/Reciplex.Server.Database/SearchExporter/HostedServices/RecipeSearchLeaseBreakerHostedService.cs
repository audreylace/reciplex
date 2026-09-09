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

        while (!stoppingToken.IsCancellationRequested)
        {
            var jitter = Random.Shared.Next(30);
            await Task.Delay(TimeSpan.FromSeconds(45 + jitter), stoppingToken);
            await BreakLeasesAsync(stoppingToken);
        }
    }

    /// <summary>
    /// Breaks leases until no more are found with a small delay between each cycle.
    /// </summary>
    /// <param name="ct">async cancellation token</param>
    private async Task BreakLeasesAsync(CancellationToken ct)
    {
        const int TenMinutesInSeconds = 10 * 60;
        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (
                    await recipeSearchExportStatusRepository.BreakLeasesAsync(
                        options.Value.LeaseBreakBatchSize,
                        TenMinutesInSeconds,
                        TenMinutesInSeconds,
                        ct
                    ) < 1
                )
                {
                    break;
                }

                if (options.Value.LeaseBreakLoopPauseMs > 0)
                {
                    await Task.Delay(options.Value.LeaseBreakLoopPauseMs, ct); // 50ms delay so the database can breathe. Important for SQLite3 backend.
                }
            }
        }
        catch (Exception ex)
            when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.Error_LeaseBreakingFailed(ex);
        }
    }
}
