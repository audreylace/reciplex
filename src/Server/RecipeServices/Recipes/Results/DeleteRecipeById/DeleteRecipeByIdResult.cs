namespace Reciplex.Server.RecipeServices.Recipes.Results.DeleteRecipeById;

/// <summary>
/// Encodes data about a recipe delete operation
/// </summary>
public class DeleteRecipeByIdResult
{
    /// <summary>
    /// The outcome of the operation
    /// </summary>
    public required DeleteRecipeByIdResultOutcome Outcome { get; init; }
}
