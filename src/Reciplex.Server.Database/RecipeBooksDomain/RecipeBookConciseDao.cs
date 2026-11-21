namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Concise description of a book
/// </summary>
public class RecipeBookConciseDao
{
    /// <summary>
    /// Recipe book primary key.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Name of the book
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Book short description
    /// </summary>
    public required string ShortDescription { get; init; }
}
