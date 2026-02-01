namespace Reciplex.Server.RecipeServices.Recipes.Results.CreateRecipe;

/// <summary>
/// Result of a create recipe outcome
/// </summary>
public enum CreateRecipeResultOutcome
{
    /// <summary>
    /// User does not have permission to add recipes to the recipe book
    /// </summary>
    LacksPermission = 1,

    /// <summary>
    /// Book does not exist or the user does not have any permissions to know the book exists
    /// </summary>
    NotFound,

    /// <summary>
    /// Argument validation failure
    /// </summary>
    ValidationFailure,

    /// <summary>
    /// Recipe added to book
    /// </summary>
    Success,
}
