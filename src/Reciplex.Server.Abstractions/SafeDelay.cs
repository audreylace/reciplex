namespace Reciplex.Server.Abstractions;

public static class SafeDelay
{
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
