using Reciplex.Server.RecipeServices.RecipeBooks.Models;

namespace Reciplex.Server.RecipeServices.RecipeBooks.Results.UpdateRecipeBook;

/// <summary>
/// Result of a update recipe result
/// </summary>
public class UpdateRecipeBookDetailsResult
{
    /// <summary>
    /// Validation errors
    /// </summary>
    public Dictionary<string, string[]> Errors { get; private init; } = [];

    /// <summary>
    /// The outcome
    /// </summary>
    public UpdateRecipeBookDetailsOutcome Outcome { get; private init; }

    /// <summary>
    /// Recipe book data. Only set on success
    /// </summary>
    public RecipeBookDao RecipeBook
    {
        get =>
            field
            ?? throw new InvalidOperationException("recipe book is only set on a success result");
        private set;
    }

    /// <summary>
    /// Success constructor
    /// </summary>
    /// <param name="recipeDao">New state of the recipe</param>
    public UpdateRecipeBookDetailsResult(RecipeBookDao recipeDao)
    {
        RecipeBook = recipeDao;
        Outcome = UpdateRecipeBookDetailsOutcome.Success;
    }

    /// <summary>
    /// Validation error constructor
    /// </summary>
    /// <param name="errors">mapping of validation errors</param>
    public UpdateRecipeBookDetailsResult(Dictionary<string, string[]> errors)
    {
        Errors = errors;
        Outcome = UpdateRecipeBookDetailsOutcome.ValidationFailure;
    }

    /// <summary>
    /// Sets any outcome but success
    /// </summary>
    /// <param name="outcome">the outcome of the operation</param>
    /// <exception cref="ArgumentException">thrown if <paramref name="outcome"/> is set to <see cref="UpdateRecipeBookDetailsOutcome.Success"/></exception>
    public UpdateRecipeBookDetailsResult(UpdateRecipeBookDetailsOutcome outcome)
    {
        if (outcome == UpdateRecipeBookDetailsOutcome.Success)
        {
            throw new ArgumentException("use success constructor", nameof(outcome));
        }
        Outcome = outcome;
    }
}
