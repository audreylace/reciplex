using Reciplex.Server.RecipeServices.RecipeBooks.Models;

namespace Reciplex.Server.RecipeServices.RecipeBooks.Results.UpdateRecipeBook;

/// <summary>
/// A recipe book update success
/// </summary>
public class UpdateRecipeBookDetailsSuccess : IUpdateRecipeBookDetailsResults
{
    /// <summary>
    /// The new state of the book
    /// </summary>
    public required RecipeBookDao RecipeBook { get; init; }
}
