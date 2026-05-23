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
    /// <param name="recipeKey">the recipe id</param>
    /// <param name="userKey">id of the requesting user for access control</param>
    /// <param name="cancellationToken">cancels the async action</param>
    /// <returns>the recipe if found. null if not found or if the user does not have access</returns>
    public Task<
        DatabaseResultVariant<SuccessResult<RecipeDao>, NotFoundResult, UserNotFoundResult>
    > GetRecipeAsync(string recipeKey, string userKey, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a recipe
    /// </summary>
    /// <param name="recipeKey">the recipe id</param>
    /// <param name="userKey">id of the requesting user for access control</param>
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
    > DeleteRecipeAsync(
        string recipeKey,
        string userKey,
        string concurrencyTag,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Updates a recipe
    /// </summary>
    /// <param name="recipeKey">the recipe id</param>
    /// <param name="userKey">id of the requesting user for access control</param>
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
        string recipeKey,
        string userKey,
        string concurrencyTag,
        UpdateRecipeArgs args,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Creates a recipe
    /// </summary>
    /// <param name="bookKey">the recipe book that the recipe will get added to</param>
    /// <param name="userKey">id of the requesting user for access control</param>
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
        string bookKey,
        string userKey,
        CreateRecipeArgs args,
        CancellationToken cancellationToken
    );
    Task<
        DatabaseResultVariant<
            SuccessResult<List<RecipeListEntryDao>>,
            NotFoundResult,
            UserNotFoundResult,
            ValidationFailureResult
        >
    > ListRecipesAsync(
        string userKey,
        ListRecipesArgs args,
        CancellationToken cancellationToken = default
    );
}
