namespace Reciplex.Server.Database.SearchExporter;

public interface IRecipeMutationNotifyService
{
    void NotifyOne();
    Task WaitForChange(CancellationToken ct);
    bool DrainUpToChange(int count);
    Task WaitForNew(CancellationToken ct);
    bool DrainUpToNew(int count);
    void NotifyNew();
}
