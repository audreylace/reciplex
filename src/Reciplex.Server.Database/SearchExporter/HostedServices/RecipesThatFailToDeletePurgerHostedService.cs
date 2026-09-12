using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter.HostedServices;

/// <summary>
/// Background service that purges recipes with too many
/// delete attempts.
/// </summary>
/// <param name="repository">repository managing recipe search rows</param>
/// <param name="options">options controlling the behavior of this background service</param>
/// <param name="logger">logger for the hosted service</param>
class RecipesThatFailToDeletePurgerHostedService(
    IRecipeSearchExportStatusRepository repository,
    IOptions<SearchExporterOptions> options,
    ILogger<RecipesThatFailToDeletePurgerHostedService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable)
        {
            return;
        }

        PeriodicTimer periodicTimer = new(TimeSpan.FromMinutes(1));
        while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                while (
                    !stoppingToken.IsCancellationRequested
                    && await repository.PurgeRecipeSearchEntriesWithTooManyRetries(
                        options.Value.RecipesFailedToDeletePurgeSize,
                        options.Value.MaxRecipeDeleteAttempts,
                        stoppingToken
                    ) > 0
                ) { }
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException || !stoppingToken.IsCancellationRequested
                )
            {
                logger.Error_PurgingRecipesFailed(ex);
            }
        }
    }
}
