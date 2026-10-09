namespace Reciplex.Server.Database.SearchExporter.Repositories;

/// <summary>
/// The kind of <see cref="RecipeBookSearchIndexDocument"/>
/// </summary>
enum RecipeBookSearchIndexDocumentKind
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
