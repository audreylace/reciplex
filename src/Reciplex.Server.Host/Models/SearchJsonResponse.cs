namespace Reciplex.Server.Host.Models;

/// <summary>
/// Model of the response from the search controller endpoint
/// </summary>
public class SearchJsonResponse
{
    /// <summary>
    /// Search hits in the order as returned from the search application
    /// </summary>
    public required List<HitsPtrJson> Hits { get; init; }

    /// <summary>
    /// Recipe data pulled from the search index
    /// </summary>
    public required List<RecipeListEntryJsonResponse> Recipes { get; init; }

    /// <summary>
    /// Book data pulled from the search index and the database
    /// </summary>
    public required List<ConciseRecipeBookJsonResponse> Books { get; init; }
}
