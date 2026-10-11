using System.Threading.Channels;

namespace Reciplex.Server.Database;

static class ChannelExtensions
{
    public static async Task WaitOnChannel<T>(
        this Channel<T> channel,
        TimeSpan timeout,
        CancellationToken ct
    )
    {
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
