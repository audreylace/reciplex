using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter.SearchBackgroundTasks;

/// <summary>
/// exports recipes with changes to the search index
/// </summary>
/// <param name="exportStatusRepository">repository holding rows tracking search export status</param>
/// <param name="options">search options</param>
/// <param name="tagProvider">generates random tag</param>
/// <param name="logger">hosted service logger</param>
/// <param name="notifyService">notifies when recipes have changed</param>
/// <param name="searchExporterStrategy">strategy for exporting recipes to the search index</param>
sealed class ChangedRecipeExporterSearchBackgroundTask(
    IRecipeSearchExportStatusRepository exportStatusRepository,
    IOptions<SearchExporterOptions> options,
    IConcurrencyTagProvider tagProvider,
    ILogger<ChangedRecipeExporterSearchBackgroundTask> logger,
    IRecipeMutationNotifyService notifyService,
    RecipeSearchIndexExporterStrategy searchExporterStrategy
) : ISearchBackgroundTask
{
    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                string leaseToken = tagProvider.NextTag();
                List<long> recipeIds = await exportStatusRepository.GetRecipesToExtractAsync(
                    options.Value.SearchExportBatchSize,
                    options.Value.MaxExportAttempts,
                    stoppingToken
                );
                if (recipeIds.Count > 0)
                {
                    int claimCount = await exportStatusRepository.ClaimAsync(
                        recipeIds,
                        leaseToken,
                        TimeSpan.FromMinutes(5),
                        stoppingToken
                    );
                    if (claimCount > 0)
                    {
                        await searchExporterStrategy.ExportRecipesAsync(
                            recipeIds,
                            leaseToken,
                            stoppingToken
                        );
                    }
                }
                else
                {
                    await notifyService.WaitForChange(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return; // exit background service
            }
            catch (Exception ex)
            {
                logger.Error_ChangedRecipeExportLoopFailed(ex);
                await SafeDelay.DelayAsync(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
