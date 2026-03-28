namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Provides services for interacting with recipes
/// </summary>
public interface IRecipesRepository
{
    /// <summary>
    /// Gets a recipe by Id
    /// </summary>
    /// <param name="recipeId">the recipe id</param>
    /// <param name="userId">id of the requesting user for access control</param>
    /// <param name="cancellationToken">cancels the async action</param>
    /// <returns>the recipe if found. null if not found or if the user does not have access</returns>
    public Task<RecipeDao?> GetRecipeAsync(
        string recipeId,
        string userId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Deletes a recipe
    /// </summary>
    /// <param name="recipeId">the recipe id</param>
    /// <param name="userId">id of the requesting user for access control</param>
    /// <param name="concurrencyTag">recipe concurrency tag to ensure the remote is deleting the recipe they observed</param>
    /// <param name="cancellationToken">cancels the async action</param>
    /// <returns>task that resolves to the outcome of the async operation</returns>
    public Task<DeleteRecipeByIdResult> DeleteRecipeAsync(
        string recipeId,
        string userId,
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
    public Task<UpdateRecipeResult> UpdateRecipeAsync(
        string recipeId,
        string userId,
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
    public Task<CreateRecipeResult> CreateRecipeAsync(
        string bookId,
        string userId,
        CreateRecipeArgs args,
        CancellationToken cancellationToken
    );
    IAsyncEnumerable<RecipeDao> ListRecipesAsync(
        string userId,
        ListRecipesArgs args,
        CancellationToken cancellationToken = default
    );
}
