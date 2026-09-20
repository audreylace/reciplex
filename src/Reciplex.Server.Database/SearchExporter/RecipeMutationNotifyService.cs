using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Reciplex.Server.Database.SearchExporter;

public class RecipeMutationNotifyService(IOptions<SearchExporterOptions> options)
    : IRecipeMutationNotifyService
{
    private readonly Channel<long> _changeChannel = Channel.CreateBounded<long>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    private readonly Channel<long> _newChannel = Channel.CreateBounded<long>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    public void NotifyNew()
    {
        if (!options.Value.Enable)
        {
            return;
        }
        _newChannel.Writer.TryWrite(0);
    }

    public void NotifyChange()
    {
        if (!options.Value.Enable)
        {
            return;
        }
        _changeChannel.Writer.TryWrite(0);
    }

    public async Task WaitForChange(TimeSpan wait, CancellationToken ct)
    {
        if (!options.Value.Enable)
        {
            throw new InvalidOperationException();
        }

        using CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(ct);
        cancellationTokenSource.CancelAfter(wait);
        try
        {
            while (
                !cancellationTokenSource.IsCancellationRequested
                && await _changeChannel.Reader.WaitToReadAsync(cancellationTokenSource.Token)
                && _changeChannel.Reader.TryRead(out _)
            ) { }
        }
        catch (OperationCanceledException)
            when (cancellationTokenSource.Token.IsCancellationRequested
                && !ct.IsCancellationRequested
            )
        {
            return;
        }
    }

    public async Task WaitForNew(TimeSpan timeout, CancellationToken ct)
    {
        if (!options.Value.Enable)
        {
            throw new InvalidOperationException();
        }

        using CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(ct);
        cancellationTokenSource.CancelAfter(timeout);
        try
        {
            while (
                !cancellationTokenSource.IsCancellationRequested
                && await _newChannel.Reader.WaitToReadAsync(cancellationTokenSource.Token)
                && _newChannel.Reader.TryRead(out _)
            ) { }
        }
        catch (OperationCanceledException)
            when (cancellationTokenSource.Token.IsCancellationRequested
                && !ct.IsCancellationRequested
            )
        {
            return;
        }
    }
}
