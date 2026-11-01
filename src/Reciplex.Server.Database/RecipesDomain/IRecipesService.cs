using Reciplex.Server.Database.Results;

namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Provides services for interacting with recipes
/// </summary>
public interface IRecipesService
{
    /// <summary>
    /// Gets a recipe by Id
    /// </summary>
    /// <param name="recipeId">the recipe id</param>
    /// <param name="userId">id of the requesting user for access control</param>
    /// <param name="cancellationToken">cancels the async action</param>
    /// <returns>the recipe if found. null if not found or if the user does not have access</returns>
    public Task<
        DatabaseResultVariant<SuccessResult<RecipeDao>, NotFoundResult, UserNotFoundResult>
    > GetRecipeAsync(long recipeId, long userId, CancellationToken cancellationToken);

    /// <summary>
    /// Queues a recipe's deletion
    /// </summary>
    /// <param name="recipeId">the recipe id</param>
    /// <param name="userId">id of the requesting user for access control</param>
    /// <param name="concurrencyTag">recipe concurrency tag to ensure the remote is deleting the recipe they observed</param>
    /// <param name="cancellationToken">cancels the async action</param>
    /// <returns>task that resolves to the outcome of the async operation</returns>
    public Task<
        DatabaseResultVariant<
            EmptySuccessResult,
            NotFoundResult,
            ForbiddenResult,
            UserNotFoundResult,
            ConflictResult
        >
    > QueueRecipeDeleteAsync(
        long recipeId,
        long userId,
        string concurrencyTag,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Updates a recipe
    /// </summary>
    /// <param name="recipeId">the recipe id</param>
    /// <param name="userId">id of the requesting user for access control</param>
    /// <param name="concurrencyTag">recipe concurrency tag to ensure the remote is deleting the recipe they observed</param>
    /// <param name="args">Additional recipe data</param>
    /// <param name="cancellationToken">cancels the async action</param>
    /// <returns>task that resolves to the outcome of the async operation</returns>
    public Task<
        DatabaseResultVariant<
            SuccessResult<RecipeDao>,
            ForbiddenResult,
            NotFoundResult,
            UserNotFoundResult,
            ValidationFailureResult,
            ConflictResult
        >
    > UpdateRecipeAsync(
        long recipeId,
        long userId,
        string concurrencyTag,
        UpdateRecipeArgs args,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Creates a recipe
    /// </summary>
    /// <param name="bookId">the recipe book that the recipe will get added to</param>
    /// <param name="userId">id of the requesting user for access control</param>
    /// <param name="args">Additional recipe data</param>
    /// <param name="cancellationToken">cancels the async action</param>
    /// <returns>task that resolves to the outcome of the async operation</returns>
    public Task<
        DatabaseResultVariant<
            SuccessResult<RecipeDao>,
            ForbiddenResult,
            NotFoundResult,
            UserNotFoundResult,
            ValidationFailureResult
        >
    > CreateRecipeAsync(
        long bookId,
        long userId,
        CreateRecipeArgs args,
        CancellationToken cancellationToken
    );
    Task<
        DatabaseResultVariant<
            SuccessResult<List<RecipeListEntryDao>>,
            NotFoundResult,
            UserNotFoundResult
        >
    > ListRecipesAsync(
        long userId,
        ListRecipesArgs args,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Looks for recipes using the search backend. This is an optional feature and is not always enabled.
    /// </summary>
    /// <param name="userId">The key of the acting user</param>
    /// <param name="searchString">the user's search string</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>result of the operation</returns>
    Task<
        DatabaseResultVariant<
            SuccessResult<List<RecipeListEntryDao>>,
            FeatureNotEnabledResult,
            BookNotFoundResult,
            UserNotFoundResult,
            ValidationFailureResult
        >
    > SearchRecipesAsync(long userId, SearchRecipesArgs args, CancellationToken ct);

    /// <summary>
    /// Removes recipes with the delete status from the database that do not have associated dependent
    /// records.
    /// </summary>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows dropped</returns>
    Task<int> PurgeAllDeletedUnreferencedRecipesAsync(CancellationToken ct);

    /// <summary>
    /// Removes recipes with the delete status from the database that do not have associated dependent
    /// records.
    /// </summary>
    /// <param name="bookId">the book to scope to</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows dropped</returns>
    Task<int> PurgeAllDeletedUnreferencedRecipesInBookAsync(long bookId, CancellationToken ct);

    /// <summary>
    /// Marks recipes in book as deleted. Caller needs to invoke <see cref="IRecipeSearchExportStatusRepository.MarkExtractionStatusForBookAsPendingDeleteAsync"/>
    /// to trigger search cleanup.
    /// </summary>
    /// <param name="bookId">the id of the book.</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>number of rows queued</returns>
    Task<int> QueueAllRecipesInBookForDeletion(long bookId, CancellationToken ct);

    /// <summary>
    /// Checks if there are any recipes in the book both deleted and undeleted
    /// </summary>
    /// <param name="bookId">the id of the book</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>result of the check</returns>
    Task<bool> AnyRecipesInBookAsync(long bookId, CancellationToken ct);
}
