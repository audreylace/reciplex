namespace Reciplex.Server.SearchExporter;

interface ISearchIndexExporterScopeFactory
{
    Task CreateAndExecuteOneAsync(CancellationToken ct);
}
