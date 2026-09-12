using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Reciplex.Server.Database.SearchExporter;

public class RecipeMutationNotifyService(IOptions<SearchExporterOptions> options)
    : IRecipeMutationNotifyService
{
    private readonly Channel<long> _changeChannel = Channel.CreateBounded<long>(
        new BoundedChannelOptions(32)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    private readonly Channel<long> _newChannel = Channel.CreateBounded<long>(
        new BoundedChannelOptions(32)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    public bool DrainUpToChange(int count)
    {
        int counter = 0;
        while (_changeChannel.Reader.TryRead(out _) && counter < count)
        {
            counter++;
        }
        return counter > 0;
    }

    public bool DrainUpToNew(int count)
    {
        int counter = 0;
        while (_newChannel.Reader.TryRead(out _) && counter < count)
        {
            counter++;
        }
        return counter > 0;
    }

    public void NotifyNew()
    {
        if (!options.Value.Enable)
        {
            return;
        }
        _newChannel.Writer.TryWrite(0);
    }

    public void NotifyOne()
    {
        if (!options.Value.Enable)
        {
            return;
        }
        _changeChannel.Writer.TryWrite(0);
    }

    public async Task WaitForChange(CancellationToken ct)
    {
        if (!options.Value.Enable)
        {
            throw new InvalidOperationException();
        }

        while (!ct.IsCancellationRequested && await _changeChannel.Reader.WaitToReadAsync(ct))
        {
            if (_changeChannel.Reader.TryRead(out _))
            {
                return;
            }
            ct.ThrowIfCancellationRequested();
        }
    }

    public async Task WaitForNew(CancellationToken ct)
    {
        if (!options.Value.Enable)
        {
            throw new InvalidOperationException();
        }

        while (!ct.IsCancellationRequested && await _newChannel.Reader.WaitToReadAsync(ct))
        {
            if (_changeChannel.Reader.TryRead(out _))
            {
                return;
            }
            ct.ThrowIfCancellationRequested();
        }
    }
}
