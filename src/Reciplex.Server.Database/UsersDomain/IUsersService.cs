using Reciplex.Server.Database.Results;

namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Service for storing, retrieving, and manipulating user records
/// </summary>
public interface IUsersService
{
    /// <summary>
    /// Looks up a user by <paramref name="userKey"/>
    /// </summary>
    /// <param name="userKey">the user identifier</param>
    /// <param name="cancellationToken">async cancellation token</param>
    /// <returns>information about the user or null if the user does not exist</returns>
    Task<DatabaseResultVariant<SuccessResult<UserDao>, UserNotFoundResult>> GetUserAsync(
        string userKey,
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
    /// <param name="userKey">the user to check</param>
    /// <param name="ct">cancellation token for the async operation</param>
    /// <returns>true if the pair grant access to <paramref name="userKey"/></returns>
    /// <remarks>
    /// A given subject and authority grants access to a set of users. This API is used
    /// to determine if the pair would allow access as the specified user.
    /// </remarks>
    Task<
        DatabaseResultVariant<SuccessResult<UserDao>, UserNotFoundResult, ForbiddenResult>
    > CheckAuthorizationAsync(
        string authority,
        string subject,
        string userKey,
        CancellationToken ct
    );

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="userKey">the user key to delete</param>
    /// <param name="concurrencyToken">the concurrency token</param>
    /// <param name="ct">cancellation token for the async operation</param>
    /// <returns>task that resolves on operation completion with information on the outcome</returns>
    Task<
        DatabaseResultVariant<EmptySuccessResult, ConflictResult, UserNotFoundResult>
    > DeleteUserAsync(string userKey, string concurrencyToken, CancellationToken ct);

    /// <summary>
    /// Updates a user
    /// </summary>
    /// <param name="userKey">the key of the user to update</param>
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
        string userKey,
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
}
