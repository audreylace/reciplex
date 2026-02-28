using Reciplex.Server.RecipeServices.Recipes.Models;
using Reciplex.Server.RecipeServices.Recipes.Results.UpdateRecipe;

namespace Reciplex.Server.RecipeServices.Recipes.Results.CreateRecipe;

/// <summary>
/// Result of a create recipe operation
/// </summary>
public class CreateRecipeResult
{
    /// <summary>
    /// The outcome
    /// </summary>
    public CreateRecipeResultOutcome Outcome { get; private init; }

    /// <summary>
    /// Validation errors
    /// </summary>
    public Dictionary<string, string[]> Errors { get; private init; } = [];

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
    public CreateRecipeResult(RecipeDao recipeDao)
    {
        Recipe = recipeDao;
        Outcome = CreateRecipeResultOutcome.Success;
    }

    /// <summary>
    /// Validation error constructor
    /// </summary>
    /// <param name="errors">mapping of validation errors</param>
    public CreateRecipeResult(Dictionary<string, string[]> errors)
    {
        Errors = errors;
        Outcome = CreateRecipeResultOutcome.ValidationFailure;
    }

    /// <summary>
    /// Sets any outcome but success
    /// </summary>
    /// <param name="outcome">the outcome of the operation</param>
    /// <exception cref="ArgumentException">thrown if <paramref name="outcome"/> is set to <see cref="UpdateRecipeResultOutcome.Success"/></exception>
    public CreateRecipeResult(CreateRecipeResultOutcome outcome)
    {
        if (outcome == CreateRecipeResultOutcome.Success)
        {
            throw new ArgumentException("use success constructor", nameof(outcome));
        }
        Outcome = outcome;
    }

    public void EnsureSuccess()
    {
        if (Outcome != CreateRecipeResultOutcome.Success)
        {
            throw new Exception($"Create recipe operation failed with outcome {Outcome}");
        }
    }
}
