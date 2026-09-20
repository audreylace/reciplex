using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Reciplex.Server.Database.SearchExporter;

class RecipeIndexCreationCoordinator(
    IHostApplicationLifetime lifetime,
    IOptions<SearchExporterOptions> options
) : IDisposable, IRecipeIndexCreationCoordinator
{
    private readonly CancellationTokenSource _tokenSource =
        CancellationTokenSource.CreateLinkedTokenSource(lifetime.ApplicationStopping);

    public async Task<bool> WaitForIndexSetupAsync(CancellationToken ct)
    {
        ThrowIfDisabled();

        if (lifetime.ApplicationStopping.IsCancellationRequested)
        {
            return false;
        }

        if (_tokenSource.IsCancellationRequested)
        {
            return true;
        }
        using CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(_tokenSource.Token, ct);
        await SafeDelay.DelayAsync(TimeSpan.MaxValue, cancellationTokenSource.Token);

        return !lifetime.ApplicationStopping.IsCancellationRequested;
    }

    private void ThrowIfDisabled()
    {
        if (!options.Value.Enable)
        {
            throw new InvalidOperationException("Search is not enabled");
        }
    }

    public async Task DeclareIndexSetupAsync()
    {
        ThrowIfDisabled();
        await _tokenSource.CancelAsync();
    }

    public void Dispose()
    {
        _tokenSource.Dispose();
    }
}
