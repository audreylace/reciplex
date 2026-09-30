namespace Reciplex.Server.Database.SearchExporter;

public interface IRecipeMutationNotifyService
{
    void NotifyChange();
    Task WaitForChange(TimeSpan timeout, CancellationToken ct);
    void NotifyDelete();
    Task WaitForDeleteAsync(TimeSpan timeout, CancellationToken ct);
}
