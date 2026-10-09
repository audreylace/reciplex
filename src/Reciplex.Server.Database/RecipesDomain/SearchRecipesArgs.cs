namespace Reciplex.Server.Database.RecipesDomain;

public class SearchRecipesArgs
{
    public required string SearchString { get; init; }

    /// <summary>
    /// Limits the search to one or more books.
    /// </summary>
    public List<string> Books { get; init; } = [];

    /// <summary>
    /// The max number of books to include in the search. Consumed when <see cref="Books"/>
    /// is not set. Higher numbers mean more ram and slower search queries. However,
    /// low numbers mean some of the user's recipes could be ignored since
    /// the search backend only searches books the user has access to and this
    /// number limits how many books we tell it the user has access to.
    /// </summary>
    public int MaxBooks { get; init; } = 1000;

    /// <summary>
    /// Max number of results to return
    /// </summary>
    public int MaxResults { get; init; } = 100;
}
