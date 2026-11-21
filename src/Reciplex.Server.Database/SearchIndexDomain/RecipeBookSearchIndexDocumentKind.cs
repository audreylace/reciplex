namespace Reciplex.Server.Database.SearchIndexDomain;

/// <summary>
/// The kind of <see cref="RecipeBookSearchIndexDocument"/>
/// </summary>
public enum RecipeBookSearchIndexDocumentKind
{
    /// <summary>
    /// Entry represents a recipe book
    /// </summary>
    RecipeBook = 1,

    /// <summary>
    /// Entry represents a recipe
    /// </summary>
    Recipe = 2,
}
