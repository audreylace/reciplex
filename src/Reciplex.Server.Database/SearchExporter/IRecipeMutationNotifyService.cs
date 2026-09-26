namespace Reciplex.Server.Database.SearchExporter;

public interface IRecipeMutationNotifyService
{
    void NotifyChange();
    Task WaitForChange(TimeSpan timeout, CancellationToken ct);
    Task WaitForNew(TimeSpan timeout, CancellationToken ct);
    void NotifyNew();
}
