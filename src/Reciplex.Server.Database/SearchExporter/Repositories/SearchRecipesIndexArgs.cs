namespace Reciplex.Server.Database.SearchExporter.Repositories;

/// <summary>
/// Args for running a query on the search index
/// </summary>
class SearchRecipesIndexArgs
{
    /// <summary>
    /// The search string
    /// </summary>
    public required string SearchString { get; init; }

    /// <summary>
    /// Null means no filter. Empty will always result in no matches.
    /// </summary>
    public List<long>? BookIds { get; init; }

    public int? MaxResults { get; init; }
}
