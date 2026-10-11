using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Database.SearchIndexDomain;
using Reciplex.Server.Database.SearchIndexTrackingDomain;
using Reciplex.Server.SearchExporter.Loggers;
using Reciplex.Server.SearchExporter.Options;
using Reciplex.Server.SearchExporter.SearchBackgroundTasks;

namespace Reciplex.Server.SearchExporter;

class SearchIndexExporterScope(
    IEnumerable<ISearchBackgroundTask> searchBackgroundTasks,
    ISearchIndexRepository searchIndexRepository,
    ILogger<SearchIndexExporterScope> logger,
    IOptions<SearchExporterOptions> options,
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository
) : ISearchIndexExporterScope
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        if (!options.Value.Enable)
        {
            await DeleteUnusedRecipeSearchEntriesLoopAsync(ct);
            return;
        }

        if (options.Value.PauseSync)
        {
            return;
        }

        if (await searchIndexRepository.EnsureSetupCompleteAsync(ct))
        {
            await RunTasksAndCheckAsync(ct);
        }
    }

    private async Task DeleteUnusedRecipeSearchEntriesLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await recipeSearchExportStatusRepository.WaitForSearchDeleteWorkAsync(
                    TimeSpan.FromMinutes(1),
                    ct
                );
                await recipeSearchExportStatusRepository.PurgeMarkedForDeletionAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.Error_SearchEntryDeletionFailed(ex);
            }
        }
    }

    private async Task RunTasksAndCheckAsync(CancellationToken ct)
    {
        using CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(ct);
        using PeriodicTimer periodicTimer = new(TimeSpan.FromMinutes(1));
        List<Task> backgroundTasks = [];
        try
        {
            foreach (var task in searchBackgroundTasks)
            {
                backgroundTasks.Add(task.ExecuteAsync(cancellationTokenSource.Token));
            }

            if (backgroundTasks.Count > 0)
            {
                await Task.WhenAny(backgroundTasks);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // normal shutdown
        }
        catch (Exception ex)
            when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.Error_BackgroundTaskMonitorFailed(ex);
        }
        finally
        {
            await cancellationTokenSource.CancelAsync();
            foreach (var task in backgroundTasks)
            {
                try
                {
                    await task;
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    logger.Error_BackgroundTaskFailed(ex);
                }
            }
        }
    }
}
