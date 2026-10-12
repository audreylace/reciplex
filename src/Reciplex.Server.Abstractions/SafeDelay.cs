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
    /// <returns>true if an internal exception was not captured. false if an unexpected exception was handled.</returns>
    public static async Task<bool> DelayAsync(TimeSpan delay, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return true;
        }

        try
        {
            await Task.Delay(delay, ct);
            return true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
