using Reciplex.Server.RecipeServices.Recipes.Models;

namespace Reciplex.Server.RecipeServices.Recipes.Results.UpdateRecipe;

/// <summary>
/// Result of a update recipe result
/// </summary>
public class UpdateRecipeResult
{
    /// <summary>
    /// Validation errors
    /// </summary>
    public Dictionary<string, string[]> Errors { get; private init; } = [];

    /// <summary>
    /// The outcome
    /// </summary>
    public UpdateRecipeResultOutcome Outcome { get; private init; }

    /// <summary>
    /// Recipe data. Only set on success
    /// </summary>
    public RecipeDao Recipe
    {
        get =>
            field ?? throw new InvalidOperationException("recipe is only set on a success result");
        private set;
    }

    /// <summary>
    /// Success constructor
    /// </summary>
    /// <param name="recipeDao">New state of the recipe</param>
    public UpdateRecipeResult(RecipeDao recipeDao)
    {
        Recipe = recipeDao;
        Outcome = UpdateRecipeResultOutcome.Success;
    }

    /// <summary>
    /// Validation error constructor
    /// </summary>
    /// <param name="errors">mapping of validation errors</param>
    public UpdateRecipeResult(Dictionary<string, string[]> errors)
    {
        Errors = errors;
        Outcome = UpdateRecipeResultOutcome.ValidationFailure;
    }

    /// <summary>
    /// Sets any outcome but success
    /// </summary>
    /// <param name="outcome">the outcome of the operation</param>
    /// <exception cref="ArgumentException">thrown if <paramref name="outcome"/> is set to <see cref="UpdateRecipeResultOutcome.Success"/></exception>
    public UpdateRecipeResult(UpdateRecipeResultOutcome outcome)
    {
        if (outcome == UpdateRecipeResultOutcome.Success)
        {
            throw new ArgumentException("use success constructor", nameof(outcome));
        }
        Outcome = outcome;
    }
}
