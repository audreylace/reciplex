namespace Reciplex.Server.Database.SearchIndexDomain;

/// <summary>
/// Pointer to a document in the search index
/// </summary>
public abstract class SearchIndexDocumentPointer
{
    /// <summary>
    /// The id of the search tracking row. This is also the search document id
    /// </summary>
    public required long RowId { get; init; }

    /// <summary>
    /// Kind of the document
    /// </summary>
    public abstract RecipeBookSearchIndexDocumentKind Kind { get; }
}
