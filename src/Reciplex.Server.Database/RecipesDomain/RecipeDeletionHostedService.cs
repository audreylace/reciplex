using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Database.SearchIndexTrackingDomain;

namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Background service that does final recipe purging from the database
/// </summary>
/// <param name="recipesService">repository holding recipes</param>
/// <param name="recipeSearchExportStatusRepository">repository for recipe search extraction status</param>
/// <param name="logger">logger for this instance</param>
sealed class RecipeDeletionHostedService(
    IRecipesService recipesService,
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository,
    ILogger<RecipeDeletionHostedService> logger
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.Info_StartingRecipePurgeWorker();
        using PeriodicTimer periodicTimer = new(TimeSpan.FromMinutes(1));
        Stopwatch stopwatch = new();
        stopwatch.Start();

        while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                if (stopwatch.Elapsed > TimeSpan.FromMinutes(10))
                {
                    stopwatch.Restart();
                    await recipeSearchExportStatusRepository.CrossSyncRecipesDeletionStatusAsync(
                        stoppingToken
                    );
                }

                await recipesService.PurgeAllDeletedUnreferencedRecipesAsync(stoppingToken);
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
