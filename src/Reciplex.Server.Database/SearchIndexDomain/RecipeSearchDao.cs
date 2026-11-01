namespace Reciplex.Server.Database.SearchIndexDomain;

/// <summary>
/// Public object representing a recipe in the search index
/// </summary>
public sealed class RecipeSearchDao : IRecipeOrBookSearchDao
{
    /// <summary>
    /// The recipe id of this search index entry.
    /// </summary>
    public required long RecipeId { get; set; }

    /// <summary>
    /// The name of the recipe
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The recipe's short description
    /// </summary>
    public required string ShortDescription { get; set; }

    /// <summary>
    /// The main parent book id.
    /// </summary>
    public required long RecipeBookId { get; set; }

    /// <summary>
    /// The source row id
    /// </summary>
    public required long RowId { get; set; }
}
