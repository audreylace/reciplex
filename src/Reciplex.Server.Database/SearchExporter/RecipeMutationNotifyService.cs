using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Reciplex.Server.Database.SearchExporter;

public class RecipeMutationNotifyService(IOptions<SearchExporterOptions> options)
    : IRecipeMutationNotifyService
{
    private readonly Channel<long> _searchIndexChangeChannel = Channel.CreateBounded<long>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    private readonly Channel<long> _searchIndexDeleteChannel = Channel.CreateBounded<long>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    private readonly Channel<long> _bookDeleteChannel = Channel.CreateBounded<long>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    public void TriggerBookDelete()
    {
        _bookDeleteChannel.Writer.TryWrite(0);
    }

    public void TriggerSearchExtraction()
    {
        if (!options.Value.Enable)
        {
            return;
        }
        _searchIndexChangeChannel.Writer.TryWrite(0);
    }

    public void TriggerSearchIndexDelete()
    {
        if (!options.Value.Enable)
        {
            return;
        }
        _searchIndexDeleteChannel.Writer.TryWrite(0);
    }

    public Task WaitForBookDeleteTriggerAsync(TimeSpan timeout, CancellationToken ct)
    {
        return WaitOnChannel(_searchIndexChangeChannel, timeout, ct);
    }

    public Task WaitForSearchExtractionTriggerAsync(TimeSpan timeout, CancellationToken ct)
    {
        return WaitOnChannel(_searchIndexChangeChannel, timeout, ct);
    }

    public Task WaitForSearchIndexDeleteTriggerAsync(TimeSpan timeout, CancellationToken ct)
    {
        return WaitOnChannel(_searchIndexDeleteChannel, timeout, ct);
    }

    private async Task WaitOnChannel(Channel<long> channel, TimeSpan timeout, CancellationToken ct)
    {
        if (!options.Value.Enable)
        {
            throw new InvalidOperationException();
        }

        await channel.WaitOnChannel(timeout, ct);
    }
}
