using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reciplex.Server.Abstractions;

namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Background service that does final recipe purging from the database
/// </summary>
/// <param name="dbFactory">factory for getting database connections</param>
/// <param name="recipesService">repository holding recipes</param>
/// <param name="logger">logger for this instance</param>
class RecipeDeletionHostedService(
    IRecipesService recipesService,
    ILogger<RecipeDeletionHostedService> logger
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.Info_StartingRecipePurgeWorker();
        using PeriodicTimer periodicTimer = new(TimeSpan.FromMinutes(1));

        while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await recipesService.PurgeDeletedUnreferencedRecipesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break; // application stopping
            }
            catch (Exception ex)
            {
                logger.Error_RecipeDeletionLoopFailed(ex);
                // delay to prevent a tight loop on repeated failure
                await SafeDelay.DelayAsync(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        logger.Info_StoppingRecipePurgeWorker();
    }
}
