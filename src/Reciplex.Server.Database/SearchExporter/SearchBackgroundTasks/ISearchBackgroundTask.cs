namespace Reciplex.Server.Database.SearchExporter.SearchBackgroundTasks;

interface ISearchBackgroundTask
{
    Task ExecuteAsync(CancellationToken stoppingToken);
}
