using Reciplex.Server.Database.Results;

namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Service providing access to recipe books
/// </summary>
public interface IRecipeBooksService
{
    /// <summary>
    /// Gets a recipe book by its ID
    /// </summary>
    /// <param name="bookKey">The string key of the recipe book</param>
    /// <param name="userKey">The string key of the user performing the operation</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns>Task holding the book if it exists</returns>
    Task<
        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            NotFoundResult,
            ValidationFailureResult,
            UserNotFoundResult
        >
    > GetRecipeBookAsync(string bookKey, string userKey, CancellationToken ct);

    /// <summary>
    /// Updates a recipe book
    /// </summary>
    /// <param name="bookKey">The id of the book</param>
    /// <param name="userKey">The id of the user performing the action for access checks</param>
    /// <param name="updateArgs">Arguments to the update transporting the new state of the book</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns>Task that resolves to the outcome of the operation</returns>
    Task<
        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            NotFoundResult,
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>,
            ValidationFailureResult,
            ConflictResult
        >
    > UpdateRecipeBookAsync(
        string bookKey,
        string userKey,
        UpdateRecipeBookArgs updateArgs,
        CancellationToken ct
    );

    /// <summary>
    /// Deletes a recipe book
    /// </summary>
    /// <param name="bookKey">The id of the book</param>
    /// <param name="userKey">The key of the user performing the action for access checks</param>
    /// <param name="ocTag">version of the tag</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns>Task that resolves to the outcome of the operation</returns>
    Task<
        DatabaseResultVariant<
            EmptySuccessResult,
            NotFoundResult,
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>,
            ValidationFailureResult,
            ConflictResult
        >
    > DeleteRecipeBookAsync(string bookKey, string userKey, string ocTag, CancellationToken ct);

    /// <summary>
    /// Creates a recipe book
    /// </summary>
    /// <param name="userKey">the user who will own the book</param>
    /// <param name="createArgs">creation arguments</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>result of the operation</returns>
    Task<
        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            UserNotFoundResult,
            ValidationFailureResult
        >
    > CreateRecipeBookAsync(string userKey, CreateRecipeBookArgs createArgs, CancellationToken ct);

    /// <summary>
    /// Returns the list of books a user has access to
    /// </summary>
    /// <param name="userKey">the user who will own the book</param>
    /// <param name="args"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<
        DatabaseResultVariant<
            SuccessResult<List<RecipeBookDao>>,
            ValidationFailureResult,
            UserNotFoundResult
        >
    > ListRecipeBooksAsync(string userKey, ListRecipeBooksArgs args, CancellationToken ct);

    /// <summary>
    /// Generates or clears the recipe book share key
    /// </summary>
    /// <param name="bookKey">The id of the book</param>
    /// <param name="userKey">user performing the action for access checks</param>
    /// <param name="ocTag">token for optimistic concurrency</param>
    /// <param name="shareKeyUpdateKind">the type of share key update</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns>outcome of the operation</returns>
    Task<
        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            NotFoundResult,
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>,
            ValidationFailureResult,
            ConflictResult
        >
    > UpdateShareKeyAsync(
        string bookKey,
        string userKey,
        string ocTag,
        BookShareKeyUpdateKind shareKeyUpdateKind,
        CancellationToken ct
    );

    /// <summary>
    /// Previews a book before requesting access
    /// </summary>
    /// <param name="bookKey">the book key</param>
    /// <param name="userKey">key of the user requesting access</param>
    /// <param name="shareKey">the share key. If not provided then this will return not-found if an entry does not exist.</param>
    /// <param name="ct">cancels the async operation</param>
    /// <returns>result of the operation</returns>
    Task<
        DatabaseResultVariant<
            SuccessResult<RecipeBookAccessRequestStatus>,
            NotFoundResult,
            ValidationFailureResult,
            UserNotFoundResult
        >
    > GetAccessStatusAsync(string bookKey, string userKey, string? shareKey, CancellationToken ct);

    /// <summary>
    /// Requests access to the book if an entry does not already exist
    /// </summary>
    /// <param name="bookKey">the id of the book</param>
    /// <param name="userKey">the user key</param>
    /// <param name="shareKey">the share key of the book. The request will be rejected if the key does not match</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns>result of the lookup</returns>
    Task<
        DatabaseResultVariant<
            SuccessResult<RecipeBookAccessRequestStatus>,
            NotFoundResult,
            ValidationFailureResult,
            ConflictResult,
            UserNotFoundResult
        >
    > RequestAccessAsync(string bookKey, string userKey, string shareKey, CancellationToken ct);

    /// <summary>
    /// Withdraws access to the book
    /// </summary>
    /// <param name="bookKey">the id of the book</param>
    /// <param name="userKey">the user key</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns>result of the lookup</returns>
    Task<
        DatabaseResultVariant<
            EmptySuccessResult,
            NotFoundResult,
            ValidationFailureResult,
            UserNotFoundResult,
            ConflictResult
        >
    > WithdrawAccessAsync(string bookKey, string userKey, CancellationToken ct);

    /// <summary>
    /// Lists all the users with access to the recipe book who are not the owner
    /// </summary>
    /// <param name="bookKey">the id of the book</param>
    /// <param name="userKey">the id of the user running the action</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns>list of users with access</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="bookKey"/> does not map to a book that exists</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="userKey"/> does not have access to perform this operation</param>
    /// <remarks>App code should run access checks to see if the book exists and the user has access before calling this method</remarks>
    Task<
        DatabaseResultVariant<
            NotFoundResult,
            SuccessResult<List<RecipeBookUserPermissionsDao>>,
            UserNotFoundResult,
            ForbiddenResult
        >
    > ListUsersWithAccess(string bookKey, string userKey, CancellationToken ct);

    /// <summary>
    /// Adds, updates, or removes user access entries
    /// </summary>
    /// <param name="bookKey">the id of the book</param>
    /// <param name="userKey">the id of the user running the action</param>
    /// <param name="entriesToAddUpdateOrDelete">Entries to add, update, or delete. Value of null drops the entry.</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns>Result of the operation</returns>
    Task<
        DatabaseResultVariant<
            EmptySuccessResult,
            NotFoundResult,
            DatabaseResultVariant<ForbiddenResult, UserNotFoundResult>,
            ValidationFailureResult,
            ConflictResult
        >
    > UpdateUsersAccessAsync(
        string bookKey,
        string userKey,
        IEnumerable<
            KeyValuePair<string, UpdateRecipeBookUserPermissionArgs?>
        > entriesToAddUpdateOrDelete,
        CancellationToken ct
    );
}
