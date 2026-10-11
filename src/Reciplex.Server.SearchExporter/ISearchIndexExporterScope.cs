namespace Reciplex.Server.SearchExporter;

interface ISearchIndexExporterScope
{
    Task ExecuteAsync(CancellationToken ct);
}
