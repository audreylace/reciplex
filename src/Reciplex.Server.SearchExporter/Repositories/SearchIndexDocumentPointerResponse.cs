using Reciplex.Server.Database.SearchIndexDomain;

namespace Reciplex.Server.SearchExporter.Repositories;

/// <summary>
/// Raw response when doing a search index pointer lookup
/// </summary>
class SearchIndexDocumentPointerResponse
{
    /// <summary>
    /// The id of the entry.
    /// </summary>
    public required long Id { get; set; }

    /// <summary>
    /// The recipe id of this search index entry. Only set when <see cref="Kind" /> is <see cref="RecipeBookSearchIndexDocumentKind.Recipe" />.
    /// </summary>
    public long? RecipeId { get; set; }

    /// <summary>
    /// The main parent book id. Always set for both kinds.
    /// </summary>
    public required long RecipeBookId { get; set; }

    /// <summary>
    /// The kind of the document
    /// </summary>
    public required RecipeBookSearchIndexDocumentKind Kind { get; set; }
}
