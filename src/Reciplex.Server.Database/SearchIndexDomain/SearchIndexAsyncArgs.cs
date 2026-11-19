using Reciplex.Server.Database.SearchIndexDomain;

namespace Reciplex.Server.SearchExporter;

/// <summary>
/// Searches for books
/// </summary>
public class SearchIndexAsyncArgs
{
    /// <summary>
    /// User supplied search string
    /// </summary>
    public required string SearchString { get; init; }

    /// <summary>
    /// Max number of results to return
    /// </summary>
    public int Limit { get; init; } = 100;

    /// <summary>
    /// Offset in the search, used for paging
    /// </summary>
    public int Offset { get; init; }

    /// <summary>
    /// The max number of books to include in the search.
    /// Higher numbers mean more ram and slower search queries. However,
    /// low numbers mean some of the user's books could be ignored since
    /// the search backend only searches books the user has access to and this
    /// number limits how many books we tell it the user has access to.
    /// </summary>
    public int MaxBooks { get; init; } = 1000;

    /// <summary>
    /// List of book ids to limit the search to
    /// </summary>
    public List<long>? BookIds { get; set; }

    /// <summary>
    /// The document kinds. Set to limit search.
    /// </summary>
    public List<RecipeBookSearchIndexDocumentKind>? DocumentKinds { get; set; }
}
