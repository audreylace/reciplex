using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Database.SearchExporter;

namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Background service that does final recipe purging from the database
/// </summary>
/// <param name="dbFactory">factory for getting database connections</param>
/// <param name="recipeNotificationService">notification service for trigger cleanup runs on demand</param>
/// <param name="options">search options</param>
/// <param name="logger">logger for this instance</param>
class RecipePurgeWorkerHostedService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IRecipeMutationNotifyService recipeNotificationService,
    IOptions<SearchExporterOptions> options,
    ILogger<RecipePurgeWorkerHostedService> logger
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
            using var dbContext = await dbFactory.CreateDbContextAsync(ct);

            if (!options.Value.Enable) // if search extraction is not enabled then purge the extraction entries ourselves
            {
                await dbContext
                    .RecipeSearchExtractionStatusEntries.Where(e =>
                        e.ExtractionStatus == DbObjects.SearchExtractionStatus.PendingDelete
                    )
                    .ExecuteDeleteAsync(ct);
            }

            await dbContext
                .Recipes.Where(r => r.RecipeSearchExtraction == null && r.IsDeleted)
                .ExecuteDeleteAsync(ct);

            await recipeNotificationService.WaitForRecipeDeleteTriggerAsync(
                TimeSpan.FromMinutes(1),
                ct
            );
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
