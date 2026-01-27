namespace Reciplex.Server.RecipeServices.Recipes.Results.DeleteRecipeById;

/// <summary>
/// Describes the outcome of a recipe delete operation
/// </summary>
public enum DeleteRecipeByIdResultOutcome
{
    /// <summary>
    /// An internal error occurred inside the API preventing the operation from completing.
    /// </summary>
    InternalError,

    /// <summary>
    /// Recipe deleted
    /// </summary>
    Success,

    /// <summary>
    /// Recipe does not exist or the user does not have any access to the recipe so the API
    /// is pretending the recipe does not exist.
    /// </summary>
    NotFound,

    /// <summary>
    /// The user does not have permissions to delete this recipe.
    /// </summary>
    LacksPermission,

    /// <summary>
    /// Another process modified the recipe such that the provided concurrency tag did not match
    /// </summary>
    ConcurrencyConflict,
}
