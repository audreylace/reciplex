using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Reciplex.Server.Database.SearchExporter;

public class RecipeMutationNotifyService(IOptions<SearchExporterOptions> options)
    : IRecipeMutationNotifyService
{
    private readonly Channel<long> _wakeChannel = Channel.CreateBounded<long>(
        new BoundedChannelOptions(32)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    public void NotifyOne()
    {
        if (!options.Value.Enable)
        {
            return;
        }
        _wakeChannel.Writer.TryWrite(0);
    }

    public async Task WaitForChange(CancellationToken ct)
    {
        if (!options.Value.Enable)
        {
            throw new InvalidOperationException();
        }

        while (!ct.IsCancellationRequested && await _wakeChannel.Reader.WaitToReadAsync(ct))
        {
            if (_wakeChannel.Reader.TryRead(out _))
            {
                return;
            }
            ct.ThrowIfCancellationRequested();
        }
    }
}
