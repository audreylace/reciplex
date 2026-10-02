namespace Reciplex.Server.Database.SearchExporter;

interface ISearchIndexExporterScope
{
    Task ExecuteAsync(CancellationToken ct);
}
