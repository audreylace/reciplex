using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Database.SearchExporter;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Background service that does final recipe purging from the database
/// </summary>
/// <param name="dbFactory">factory for getting database connections</param>
/// <param name="recipeSearchExportStatusRepository">repository holding search extraction status information</param>
/// <param name="recipesService">repository holding recipes</param>
/// <param name="options">search options</param>
/// <param name="logger">logger for this instance</param>
class RecipeDeletionHostedService(
    IRecipesService recipesService,
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository,
    IOptions<SearchExporterOptions> options,
    ILogger<RecipeDeletionHostedService> logger
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.Info_StartingRecipePurgeWorker();

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunLoopOnceAsync(stoppingToken);
        }

        logger.Info_StoppingRecipePurgeWorker();
    }

    /// <summary>
    /// Runs one loop iteration
    /// </summary>
    /// <param name="ct">async cancellation token</param>
    private async Task RunLoopOnceAsync(CancellationToken ct)
    {
        try
        {
            if (!options.Value.Enable) // if search extraction is not enabled then purge the extraction entries ourselves
            {
                await recipeSearchExportStatusRepository.PurgeMarkedForDeletionAsync(ct);
            }

            await recipesService.PurgeDeletedUnreferencedRecipesAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return; // application stopping
        }
        catch (Exception ex)
        {
            logger.Error_RecipeDeletionLoopFailed(ex);
            // delay to prevent a tight loop on repeated failure
            await SafeDelay.DelayAsync(TimeSpan.FromSeconds(5), ct);
        }
    }
}
