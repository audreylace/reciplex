using Reciplex.Server.Database.Results;

namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Service for storing, retrieving, and manipulating user records
/// </summary>
public interface IUsersService
{
    /// <summary>
    /// Looks up a user by <paramref name="userId"/>
    /// </summary>
    /// <param name="userId">the user identifier</param>
    /// <param name="cancellationToken">async cancellation token</param>
    /// <returns>information about the user or null if the user does not exist</returns>
    Task<DatabaseResultVariant<SuccessResult<UserDao>, UserNotFoundResult>> GetUserAsync(
        long userId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Returns the list of users mapped by the authority, subject pair
    /// </summary>
    /// <param name="authority">the authority</param>
    /// <param name="subject">the user subject</param>
    /// <param name="ct">cancellation token for the async operation</param>
    /// <returns>enumerable listing the users by subject</returns>
    /// <remarks>
    /// A given subject and authority grants access to a set of users. This API is used
    /// to determine the exact set.
    /// </remarks>
    Task<List<UserDao>> GetUsersBySubjectAsync(
        string authority,
        string subject,
        CancellationToken ct = default
    );

    /// <summary>
    /// Checks if a given authority and subject pair grant access to
    /// the supplied user key.
    /// </summary>
    /// <param name="authority">the authority</param>
    /// <param name="subject">the user subject</param>
    /// <param name="userId">the user to check</param>
    /// <param name="ct">cancellation token for the async operation</param>
    /// <returns>true if the pair grant access to <paramref name="userId"/></returns>
    /// <remarks>
    /// A given subject and authority grants access to a set of users. This API is used
    /// to determine if the pair would allow access as the specified user.
    /// </remarks>
    Task<
        DatabaseResultVariant<SuccessResult<UserDao>, UserNotFoundResult, ForbiddenResult>
    > CheckAuthorizationAsync(string authority, string subject, long userId, CancellationToken ct);

    /// <summary>
    /// Queues a user for deletion
    /// </summary>
    /// <param name="userId">the user key to delete</param>
    /// <param name="concurrencyToken">the concurrency token</param>
    /// <param name="ct">cancellation token for the async operation</param>
    /// <returns>task that resolves on operation completion with information on the outcome</returns>
    Task<
        DatabaseResultVariant<EmptySuccessResult, ConflictResult, UserNotFoundResult>
    > QueueUserDeleteAsync(long userId, string concurrencyToken, CancellationToken ct);

    /// <summary>
    /// Gets the next user record queue for deletion
    /// </summary>
    /// <param name="ct">cancellation token for the async operation</param>
    /// <returns>
    /// <see cref="EmptySuccessResult"/> when
    /// there is no user to delete. Success result with the user to delete.
    /// </returns>
    Task<
        DatabaseResultVariant<EmptySuccessResult, SuccessResult<UserDao>>
    > GetNextUserQueuedForDeletionAsync(CancellationToken ct);

    /// <summary>
    /// Requeues a user for deletion
    /// </summary>
    /// <param name="userId">the user key to requeue</param>
    /// <param name="concurrencyToken">the concurrency token</param>
    /// <param name="ct">cancellation token for the async operation</param>
    /// <returns>
    /// <see cref="EmptySuccessResult"/> on success.
    /// <see cref="ConflictResult"/> if the record was modified.
    /// <see cref="ValidationFailureResult"/> is <paramref name="userId"/> is invalid.
    /// </returns>
    Task<DatabaseResultVariant<EmptySuccessResult, ConflictResult>> RequeueUserForDeletionAsync(
        long userId,
        string concurrencyToken,
        CancellationToken ct
    );

    /// <summary>
    /// Purges the user data from the database
    /// </summary>
    /// <param name="userId">the user key of the user to purge from the database</param>
    /// <param name="concurrencyToken">the concurrency token</param>
    /// <param name="ct">cancellation token for the async operation</param>
    /// <returns>
    /// <see cref="EmptySuccessResult"/> on success.
    /// <see cref="ConflictResult"/> if the record was modified.
    /// <see cref="ValidationFailureResult"/> is <paramref name="userId"/> is invalid.
    /// </returns>
    Task<DatabaseResultVariant<EmptySuccessResult, ConflictResult>> PurgeUserFromDatabaseAsync(
        long userId,
        string concurrencyToken,
        CancellationToken ct
    );

    /// <summary>
    /// Updates a user
    /// </summary>
    /// <param name="userId">the key of the user to update</param>
    /// <param name="concurrencyToken">the concurrency token</param>
    /// <param name="args">arguments for the update operation</param>
    /// <param name="ct">cancellation token for the async operation</param>
    /// <returns>task that resolves on operation completion with information on the outcome</returns>
    Task<
        DatabaseResultVariant<
            SuccessResult<UserDao>,
            ValidationFailureResult,
            UserNotFoundResult,
            ConflictResult
        >
    > UpdateUserAsync(
        long userId,
        string concurrencyToken,
        UpdateUserArgs args,
        CancellationToken ct
    );

    /// <summary>
    /// Creates a new user record
    /// </summary>
    /// <param name="args">args for the operation</param>
    /// <param name="ct">cancellation token for the async operation</param>
    /// <returns>task that resolves on operation completion with information on the outcome</returns>
    Task<DatabaseResultVariant<SuccessResult<UserDao>, ValidationFailureResult>> CreateUserAsync(
        CreateUserArgs args,
        CancellationToken ct
    );

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
