namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Args for creating a book
/// </summary>
public class CreateRecipeBookArgs
{
    /// <summary>
    /// Name of the book
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Book short description
    /// </summary>
    public required string ShortDescription { get; set; }
}
