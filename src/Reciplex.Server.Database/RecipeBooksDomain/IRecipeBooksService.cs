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
    /// <param name="bookId">The key of the recipe book</param>
    /// <param name="userId">The key of the user performing the operation</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns>Task holding the book if it exists</returns>
    Task<
        DatabaseResultVariant<SuccessResult<RecipeBookDao>, NotFoundResult, UserNotFoundResult>
    > GetRecipeBookAsync(long bookId, long userId, CancellationToken ct);

    /// <summary>
    /// Updates a recipe book
    /// </summary>
    /// <param name="bookId">The id of the book</param>
    /// <param name="userId">The id of the user performing the action for access checks</param>
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
        long bookId,
        long userId,
        UpdateRecipeBookArgs updateArgs,
        CancellationToken ct
    );

    /// <summary>
    /// Deletes a recipe book
    /// </summary>
    /// <param name="bookId">The id of the book</param>
    /// <param name="userId">The key of the user performing the action for access checks</param>
    /// <param name="ocTag">version of the tag</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns>Task that resolves to the outcome of the operation</returns>
    Task<
        DatabaseResultVariant<
            EmptySuccessResult,
            NotFoundResult,
            ForbiddenResult,
            UserNotFoundResult,
            ConflictResult
        >
    > QueueRecipeBookDeleteAsync(long bookId, long userId, string ocTag, CancellationToken ct);

    /// <summary>
    /// Re-queue a recipe book with <paramref name="bookId"/> on the deletion queue for another
    /// later attempts to see if the book may be delete.
    /// </summary>
    /// <param name="bookId">the id of the book</param>
    /// <param name="concurrencyToken">the book concurrency token</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns><see cref="EmptySuccessResult"/> on success. <see cref="ConflictResult"/>
    /// if <paramref name="concurrencyToken"/> no longer matches the book's record.
    /// </returns>
    Task<
        DatabaseResultVariant<EmptySuccessResult, ConflictResult>
    > RequeueRecipesBooksForDeletionAsync(
        long bookId,
        string concurrencyToken,
        CancellationToken ct
    );

    /// <summary>
    /// Queues every book owned by <paramref name="userId"/> for deletion
    /// </summary>
    /// <param name="userId">the id of the user</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns>Number of books queued. 0 means either user does not have any books or all of the user's books are already queued</returns>
    Task<int> QueueAllRecipesBooksOwnedByUserForDeletionAsync(long userId, CancellationToken ct);

    /// <summary>
    /// Deletes all user entries associated with <paramref name="userId"/>
    /// </summary>
    /// <param name="userId">the id of the user</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns>Number of rows dropped</returns>
    Task<int> DeleteAllRecipesBookAccessEntriesOwnedByUserAsync(long userId, CancellationToken ct);

    /// <summary>
    /// Returns if there are still any books owned by <paramref name="userId"/>.
    /// Accounts for both deleted and non-deleted books.
    /// </summary>
    /// <param name="userId">the id of the user</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns>true if the the user still owns any books</returns>
    Task<bool> HasAnyBooksAsync(long userId, CancellationToken ct);

    /// <summary>
    /// Gets the next book queued for deletion
    /// </summary>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns><see cref="EmptySuccessResult" /> if there is not
    /// another book on the queue or books on the queue are scheduled
    /// for a future time.
    /// Otherwise a <see cref="RecipeBookDeletionQueueEntry" /> wrapped in a
    /// <see cref="SuccessResult{T}>"/>.</returns>
    Task<
        DatabaseResultVariant<EmptySuccessResult, SuccessResult<RecipeBookDeletionQueueEntry>>
    > GetNextBookQueuedForDeletionAsync(CancellationToken ct);

    /// <summary>
    /// Drops recipe book with id <paramref name="bookId"/> from the database.
    /// </summary>
    /// <param name="bookId">the book id</param>
    /// <param name="concurrencyToken">the book concurrency token</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns><see cref="EmptySuccessResult"/> when the book is dropped or does not exist.
    /// <see cref="ConflictResult"/> when the book <paramref name="concurrencyToken"/>
    /// does not match, transaction fails, or the delete can not continue because of dependent entities.
    /// </returns>
    Task<DatabaseResultVariant<EmptySuccessResult, ConflictResult>> PurgeRecipeBookAsync(
        long bookId,
        string concurrencyToken,
        CancellationToken ct
    );

    /// <summary>
    /// Creates a recipe book
    /// </summary>
    /// <param name="userId">the user who will own the book</param>
    /// <param name="createArgs">creation arguments</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>result of the operation</returns>
    Task<
        DatabaseResultVariant<
            SuccessResult<RecipeBookDao>,
            UserNotFoundResult,
            ValidationFailureResult
        >
    > CreateRecipeBookAsync(long userId, CreateRecipeBookArgs createArgs, CancellationToken ct);

    /// <summary>
    /// Returns the list of books a user has access to
    /// </summary>
    /// <param name="userId">the user who will own the book</param>
    /// <param name="args"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<
        DatabaseResultVariant<
            SuccessResult<List<RecipeBookDao>>,
            ValidationFailureResult,
            UserNotFoundResult
        >
    > ListRecipeBooksAsync(long userId, ListRecipeBooksArgs args, CancellationToken ct);

    /// <summary>
    /// Generates or clears the recipe book share key
    /// </summary>
    /// <param name="bookId">The id of the book</param>
    /// <param name="userId">user performing the action for access checks</param>
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
        long bookId,
        long userId,
        string ocTag,
        BookShareKeyUpdateKind shareKeyUpdateKind,
        CancellationToken ct
    );

    /// <summary>
    /// Previews a book before requesting access
    /// </summary>
    /// <param name="bookId">the book key</param>
    /// <param name="userId">key of the user requesting access</param>
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
    > GetAccessStatusAsync(long bookId, long userId, string? shareKey, CancellationToken ct);

    /// <summary>
    /// Requests access to the book if an entry does not already exist
    /// </summary>
    /// <param name="bookId">the id of the book</param>
    /// <param name="userId">the user key</param>
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
    > RequestAccessAsync(long bookId, long userId, string shareKey, CancellationToken ct);

    /// <summary>
    /// Withdraws access to the book
    /// </summary>
    /// <param name="bookId">the id of the book</param>
    /// <param name="userId">the user key</param>
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
    > WithdrawAccessAsync(long bookId, long userId, CancellationToken ct);

    /// <summary>
    /// Lists all the users with access to the recipe book who are not the owner
    /// </summary>
    /// <param name="bookId">the id of the book</param>
    /// <param name="userId">the id of the user running the action</param>
    /// <param name="ct">token to stop the async operation</param>
    /// <returns>list of users with access</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="bookId"/> does not map to a book that exists</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="userId"/> does not have access to perform this operation</param>
    /// <remarks>App code should run access checks to see if the book exists and the user has access before calling this method</remarks>
    Task<
        DatabaseResultVariant<
            NotFoundResult,
            SuccessResult<List<RecipeBookUserPermissionsDao>>,
            UserNotFoundResult,
            ForbiddenResult
        >
    > ListUsersWithAccess(long bookId, long userId, CancellationToken ct);

    /// <summary>
    /// Adds, updates, or removes user access entries
    /// </summary>
    /// <param name="bookId">the id of the book</param>
    /// <param name="userId">the id of the user running the action</param>
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
        long bookId,
        long userId,
        IEnumerable<
            KeyValuePair<long, UpdateRecipeBookUserPermissionArgs?>
        > entriesToAddUpdateOrDelete,
        CancellationToken ct
    );

    /// <summary>
    /// Signals to background processes that there is work related to books being deleted
    /// </summary>
    void SignalDeletion();

    /// <summary>
    /// Internal background processes wait on this for a singla that there
    /// is work to do related to books being deleted.
    /// </summary>
    /// <param name="timeout">how long to wait</param>
    /// <param name="ct">cancels the operation</param>
    public Task WaitForDeleteSignal(TimeSpan timeout, CancellationToken ct);
}
