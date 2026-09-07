namespace Reciplex.Server.Database.SearchExporter;

public interface IRecipeMutationNotifyService
{
    void NotifyOne();
    Task WaitForOne(CancellationToken ct);
}
