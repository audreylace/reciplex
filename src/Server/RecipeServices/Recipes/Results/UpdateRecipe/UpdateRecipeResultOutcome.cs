namespace Reciplex.Server.RecipeServices.Recipes.Results.UpdateRecipe;

/// <summary>
/// Outcome of a recipe update action
/// </summary>
public enum UpdateRecipeResultOutcome
{
    /// <summary>
    /// Failure inside the service resulted in the update failing
    /// </summary>
    InternalError,

    /// <summary>
    /// Recipe was updated
    /// </summary>
    Success,

    /// <summary>
    /// User does not have permission to update this recipe
    /// </summary>
    LacksPermission,

    /// <summary>
    /// The recipe does not exist
    /// </summary>
    NotFound,

    /// <summary>
    /// One or more inputs were invalid. The update was aborted.
    /// </summary>
    ValidationFailure,

    /// <summary>
    /// Another process modified the recipe such that the provided concurrency tag did not match
    /// </summary>
    ConcurrencyConflict,
}
