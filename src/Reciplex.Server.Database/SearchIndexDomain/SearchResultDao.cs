using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.RecipesDomain;

namespace Reciplex.Server.SearchExporter;

/// <summary>
/// the search result
/// </summary>
public class SearchResultDao
{
    /// <summary>
    /// The hits in the order returned by the search index
    /// </summary>
    public List<SearchIdPointer> Hits { get; init; } = [];

    /// <summary>
    /// The set of recipes
    /// </summary>
    public List<RecipeListEntryDao> Recipes { get; init; } = [];

    /// <summary>
    /// the set of books
    /// </summary>
    public List<RecipeBookDao> RecipeBooks { get; init; } = [];
}
