using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.SearchIndexTrackingDomain;
using Reciplex.Server.SearchExporter.Loggers;
using Reciplex.Server.SearchExporter.Options;

namespace Reciplex.Server.SearchExporter.SearchBackgroundTasks;

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
                List<long> rowIds = await exportStatusRepository.GetPendingRecordsAsync(
                    options.Value.SearchExportBatchSize,
                    options.Value.MaxExportAttempts,
                    stoppingToken
                );
                if (rowIds.Count > 0)
                {
                    int claimCount = await exportStatusRepository.ClaimLeasesAsync(
                        rowIds,
                        leaseToken,
                        TimeSpan.FromMinutes(5),
                        stoppingToken
                    );
                    if (claimCount > 0)
                    {
                        await searchExporterStrategy.ExportRowsAsync(
                            rowIds,
                            leaseToken,
                            options.Value.MaxExportAttempts,
                            stoppingToken
                        );
                    }
                }
                else
                {
                    await exportStatusRepository.WaitForExtractionWorkAsync(
                        TimeSpan.FromMinutes(1),
                        stoppingToken
                    );
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
