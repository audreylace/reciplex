using System.Threading.Channels;

namespace Reciplex.Server.Database;

/// <summary>
/// extensions for channels
/// </summary>
static class ChannelExtensions
{
    /// <summary>
    /// Waits on a channel for the specified time period. Result is discarded
    /// </summary>
    /// <typeparam name="T">the channel type</typeparam>
    /// <param name="channel">the channel to wait on</param>
    /// <param name="timeout">how long to wait</param>
    /// <param name="ct">async cancellation token</param>
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
