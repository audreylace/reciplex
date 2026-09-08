using Microsoft.Extensions.DependencyInjection;
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
    IServiceProvider sp,
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
                await using var scope = sp.CreateAsyncScope();
                IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository =
                    scope.ServiceProvider.GetRequiredService<IRecipeSearchExportStatusRepository>();
                if (
                    await recipeSearchExportStatusRepository.BreakLeasesAsync(
                        100,
                        TenMinutesInSeconds,
                        TenMinutesInSeconds,
                        ct
                    ) < 1
                )
                {
                    break;
                }
                await Task.Delay(200, ct); // 200ms delay so the database can breathe. Important for SQLite3 backend.
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.Error_LeaseBreakingFailed(ex);
        }
    }
}
