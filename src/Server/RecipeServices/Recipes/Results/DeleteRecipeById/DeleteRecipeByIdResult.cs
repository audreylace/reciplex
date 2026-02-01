namespace Reciplex.Server.RecipeServices.Recipes.Results.DeleteRecipeById;

/// <summary>
/// Encodes data about a recipe delete operation
/// </summary>
public class DeleteRecipeByIdResult(DeleteRecipeByIdResultOutcome outcome)
{
    /// <summary>
    /// The outcome of the operation
    /// </summary>
    public DeleteRecipeByIdResultOutcome Outcome { get; init; } = outcome;
}
