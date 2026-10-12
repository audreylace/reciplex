namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Signals that new background work is available for user deletion threads
/// </summary>
public interface IUserDeletionSignal
{
    /// <summary>
    /// Signals that a user was deleted so there is now work to do
    /// </summary>
    void SignalDeletion();

    /// <summary>
    /// Waits for a deletion signal up to <paramref name="timeout"/>
    /// </summary>
    /// <param name="timeout">how long to wait</param>
    /// <param name="ct">async cancellation token</param>
    Task WaitForSignal(TimeSpan timeout, CancellationToken ct);
}
