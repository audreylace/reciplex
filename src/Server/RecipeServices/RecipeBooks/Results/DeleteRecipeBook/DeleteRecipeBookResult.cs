namespace Reciplex.Server.RecipeServices.RecipeBooks.Results.DeleteRecipeBook;

/// <summary>
/// Encodes the result of the delete recipe book
/// </summary>
public class DeleteRecipeBookResult
{
    /// <summary>
    /// The outcome of the operation
    /// </summary>
    public required DeleteRecipeBookResultOutcome Outcome { get; init; }
}
