namespace Reciplex.Server.Database.SearchIndexDomain;

/// <summary>
/// Public object representing a recipe book in the search index
/// </summary>
public sealed class RecipeBookSearchDao : IRecipeOrBookSearchDao
{
    /// <summary>
    /// The name of the book
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The book's short description
    /// </summary>
    public required string ShortDescription { get; set; }

    /// <summary>
    /// The book id
    /// </summary>
    public required long RecipeBookId { get; set; }

    /// <summary>
    /// The source row id
    /// </summary>
    public required long RowId { get; set; }
}
