using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Database.SearchIndexDomain;
using Reciplex.Server.SearchExporter.Loggers;
using Reciplex.Server.SearchExporter.Options;
using Reciplex.Server.SearchExporter.SearchBackgroundTasks;

namespace Reciplex.Server.SearchExporter.HostedServices;

/// <summary>
/// Syncs recipe and recipe book changes to the search index.
/// </summary>
/// <param name="searchBackgroundTasks">set of tasks to run</param>
/// <param name="searchIndexRepository">repository tracking the status of search synchronization</param>
/// <param name="options">search options</param>
/// <param name="logger">Logger for this instance</param>
sealed class RecipeSearchHostedBackgroundService(
    IEnumerable<ISearchBackgroundTask> searchBackgroundTasks,
    ISearchIndexRepository searchIndexRepository,
    IOptions<SearchExporterOptions> options,
    ILogger<RecipeSearchHostedBackgroundService> logger
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await searchIndexRepository.EnsureSetupCompleteAsync(stoppingToken))
                {
                    await RunTasksAndCheckAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return; // shutdown of host
            }
            catch (Exception ex)
            {
                logger.Error_MainLoopCaughtException(ex);
            }

            await SafeDelay.DelayAsync(TimeSpan.FromMinutes(1), stoppingToken);
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
        {
            logger.Error_SearchIndexMonitorThrew(ex);
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
                    logger.Error_MainLoopCaughtException(ex);
                }
            }
        }
    }
}
