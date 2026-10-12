namespace Reciplex.Server.Meilisearch;

/// <summary>
/// Search result
/// </summary>
/// <typeparam name="TIndexDocument">shape of the index document</typeparam>
public class SearchQueryResponse<TIndexDocument>
{
    /// <summary>
    /// Set of documents matching the search
    /// </summary>
    public List<TIndexDocument> Hits { get; init; } = [];
}
