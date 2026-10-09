using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter;

namespace Reciplex.Server.Database.RecipesDomain;

class RecipeDeletionWorkerHostedService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IRecipeMutationNotifyService recipeDeletionQueue,
    ILogger<RecipeDeletionWorkerHostedService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var dbContext = await dbFactory.CreateDbContextAsync(stoppingToken);
                await dbContext
                    .Recipes.Where(r => r.RecipeSearchExtraction == null && r.Deleted != null)
                    .ExecuteDeleteAsync(stoppingToken);
                await recipeDeletionQueue.WaitForRecipeDeleteTriggerAsync(
                    TimeSpan.FromMinutes(1),
                    stoppingToken
                );
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.Error_RecipeDeletionLoopFailed(ex);
                // delay to prevent a tight loop on repeated failure
                await SafeDelay.DelayAsync(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
