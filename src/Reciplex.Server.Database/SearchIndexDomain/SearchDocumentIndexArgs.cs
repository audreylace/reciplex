namespace Reciplex.Server.Database.SearchIndexDomain;

/// <summary>
/// Args for running a query on the search index
/// </summary>
public sealed class SearchDocumentIndexArgs
{
    /// <summary>
    /// The search string
    /// </summary>
    public required string SearchString { get; init; }

    /// <summary>
    /// Null means no filter. Empty will always result in no matches.
    /// </summary>
    public List<long>? BookIds { get; init; }

    /// <summary>
    /// Max number of results from the search index
    /// </summary>
    public int? MaxResults { get; init; }
}
