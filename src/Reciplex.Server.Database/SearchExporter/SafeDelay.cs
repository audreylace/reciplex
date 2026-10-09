namespace Reciplex.Server.Database.SearchExporter;

static class SafeDelay
{
    public static async Task<bool> DelayAsync(TimeSpan delay, CancellationToken ct)
    {
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
