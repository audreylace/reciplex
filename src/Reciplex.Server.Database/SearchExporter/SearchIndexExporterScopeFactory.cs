using Microsoft.Extensions.DependencyInjection;

namespace Reciplex.Server.Database.SearchExporter;

class SearchIndexExporterScopeFactory(IServiceProvider sp) : ISearchIndexExporterScopeFactory
{
    public async Task CreateAndExecuteOneAsync(CancellationToken ct)
    {
        using CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(ct);
        await using var scope = sp.CreateAsyncScope();
        try
        {
            var searchScope = scope.ServiceProvider.GetRequiredService<ISearchIndexExporterScope>();
            await searchScope.ExecuteAsync(ct);
        }
        finally
        {
            await cancellationTokenSource.CancelAsync();
        }
    }
}
