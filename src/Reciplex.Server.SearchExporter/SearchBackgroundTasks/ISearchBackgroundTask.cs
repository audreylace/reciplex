namespace Reciplex.Server.SearchExporter.SearchBackgroundTasks;

interface ISearchBackgroundTask
{
    Task ExecuteAsync(CancellationToken stoppingToken);
}
