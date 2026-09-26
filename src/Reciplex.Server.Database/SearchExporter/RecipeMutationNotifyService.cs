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

    private readonly Channel<long> _deleteChannel = Channel.CreateBounded<long>(
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

    public void NotifyDelete()
    {
        if (!options.Value.Enable)
        {
            return;
        }
        _deleteChannel.Writer.TryWrite(0);
    }

    public Task WaitForChange(TimeSpan timeout, CancellationToken ct)
    {
        return WaitOnChannel(_changeChannel, timeout, ct);
    }

    public Task WaitForNewAsync(TimeSpan timeout, CancellationToken ct)
    {
        return WaitOnChannel(_newChannel, timeout, ct);
    }

    public Task WaitForDeleteAsync(TimeSpan timeout, CancellationToken ct)
    {
        return WaitOnChannel(_deleteChannel, timeout, ct);
    }

    private async Task WaitOnChannel(Channel<long> channel, TimeSpan timeout, CancellationToken ct)
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
                && await channel.Reader.WaitToReadAsync(cancellationTokenSource.Token)
            )
            {
                if (channel.Reader.TryRead(out _))
                {
                    return;
                }
            }
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
