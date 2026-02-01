using Reciplex.Server.RecipeServices.RecipeBooks;
using Reciplex.Server.RecipeServices.Recipes.Models;
using Reciplex.Server.RecipeServices.Recipes.Results.CreateRecipe;
using Reciplex.Server.RecipeServices.Recipes.Results.DeleteRecipeById;
using Reciplex.Server.RecipeServices.Recipes.Results.UpdateRecipe;
using Reciplex.Server.UserServices;

namespace Reciplex.Server.RecipeServices.Recipes;

/// <summary>
/// Provides services for interacting with recipes
/// </summary>
public interface IRecipeService
{
    /// <summary>
    /// Gets a recipe by Id
    /// </summary>
    /// <param name="recipeId">the recipe id</param>
    /// <param name="userId">id of the requesting user for access control</param>
    /// <param name="cancellationToken">cancels the async action</param>
    /// <returns>the recipe if found. null if not found or if the user does not have access</returns>
    public Task<RecipeDao?> GetRecipeAsync(
        RecipeKey recipeId,
        UserKey userId,
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
        RecipeKey recipeId,
        UserKey userId,
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
        RecipeKey recipeId,
        UserKey userId,
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
        RecipeBookKey bookId,
        UserKey userId,
        CreateRecipeArgs args,
        CancellationToken cancellationToken
    );
    Task<IAsyncEnumerable<RecipeDao>?> ListRecipesAsync(
        RecipeBookKey bookId,
        UserKey userid,
        ListRecipesArgs args,
        CancellationToken cancellationToken
    );
}
