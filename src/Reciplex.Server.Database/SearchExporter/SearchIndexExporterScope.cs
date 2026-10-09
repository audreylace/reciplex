using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;
using Reciplex.Server.Database.SearchExporter.SearchBackgroundTasks;

namespace Reciplex.Server.Database.SearchExporter;

class SearchIndexExporterScope(
    IEnumerable<ISearchBackgroundTask> searchBackgroundTasks,
    ISearchIndexRepository searchIndexRepository,
    ILogger<SearchIndexExporterScope> logger
) : ISearchIndexExporterScope
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        if (await searchIndexRepository.EnsureRecipeIndexSetupCompleteAsync(ct))
        {
            await RunTasksAndCheckAsync(ct);
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
            backgroundTasks.Add(EnsureIndexExistsAsync(ct));

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

    private async Task EnsureIndexExistsAsync(CancellationToken ct)
    {
        using PeriodicTimer periodicTimer = new(TimeSpan.FromMinutes(1));
        while (await periodicTimer.WaitForNextTickAsync(ct))
        {
            if (!await searchIndexRepository.EnsureRecipeIndexSetupCompleteAsync(ct))
            {
                break;
            }
        }
    }
}
