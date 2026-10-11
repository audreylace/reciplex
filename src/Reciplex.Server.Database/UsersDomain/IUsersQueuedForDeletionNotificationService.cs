namespace Reciplex.Server.Database.UsersDomain;

public interface IUsersQueuedForDeletionNotificationService
{
    void TriggerWake();
    Task WaitForWake(TimeSpan timeout, CancellationToken ct);
}
