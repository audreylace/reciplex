using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;
using Reciplex.Server.Database.SearchExporter.SearchBackgroundTasks;

namespace Reciplex.Server.Database.SearchExporter.HostedServices;

/// <summary>
/// Runs search operations in the background
/// </summary>
/// <param name="searchIndexRepository">repository for search data</param>
class RecipeSearchHostedBackgroundService(
    ISearchIndexRepository searchIndexRepository,
    IServiceProvider sp,
    ILogger<RecipeSearchHostedBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await searchIndexRepository.EnsureRecipeIndexSetupCompleteAsync(stoppingToken))
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
                logger.Error_IndexOperationFailed(ex);
            }

            await SafeDelay.DelayAsync(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task RunTasksAndCheckAsync(CancellationToken ct)
    {
        using CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(ct);
        using PeriodicTimer periodicTimer = new(TimeSpan.FromMinutes(1));
        Task backgroundTask = Task.CompletedTask;
        try
        {
            backgroundTask = RunBackgroundTasksAsync(cancellationTokenSource);
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                await periodicTimer.WaitForNextTickAsync(cancellationTokenSource.Token);
                if (
                    !await searchIndexRepository.EnsureRecipeIndexSetupCompleteAsync(
                        cancellationTokenSource.Token
                    )
                )
                {
                    return;
                }
            }
        }
        catch (OperationCanceledException)
            when (cancellationTokenSource.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            // normal shutdown
        }
        catch (Exception ex)
            when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.Error_IndexOperationFailed(ex);
        }
        finally
        {
            await cancellationTokenSource.CancelAsync();

            try
            {
                await backgroundTask;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                logger.Error_BackgroundTaskMonitorFailed(ex);
            }
        }
    }

    private async Task RunBackgroundTasksAsync(CancellationTokenSource cancellationTokenSource)
    {
        await using var scope = sp.CreateAsyncScope();
        IEnumerable<ISearchBackgroundTask> searchBackgroundTasks =
            scope.ServiceProvider.GetServices<ISearchBackgroundTask>();
        List<Task> backgroundTasks = [];
        foreach (var task in searchBackgroundTasks)
        {
            backgroundTasks.Add(task.ExecuteAsync(cancellationTokenSource.Token));
        }

        if (backgroundTasks.Count < 1)
        {
            await cancellationTokenSource.CancelAsync();
            return;
        }
        try
        {
            await Task.WhenAny(backgroundTasks);
        }
        catch (Exception) { }

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
