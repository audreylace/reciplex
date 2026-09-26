namespace Reciplex.Server.Database.SearchExporter;

interface ISearchIndexExporterScopeFactory
{
    Task CreateAndExecuteOneAsync(CancellationToken ct);
}
