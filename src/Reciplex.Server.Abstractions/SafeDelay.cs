namespace Reciplex.Server.Abstractions;

/// <summary>
/// Functionality for a delay that does not throw an exception
/// </summary>
public static class SafeDelay
{
    /// <summary>
    /// Delays the tasks for <paramref name="delay"/> or when <paramref name="ct"/>
    /// is cancelled.
    /// </summary>
    /// <param name="delay">how long to delay</param>
    /// <param name="ct">async cancellation token</param>
    public static async Task DelayAsync(TimeSpan delay, CancellationToken ct)
    {
        if (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(delay, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        }
    }
}
