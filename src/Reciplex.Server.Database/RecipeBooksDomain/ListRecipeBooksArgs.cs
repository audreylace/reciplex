namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Arguments for listing books
/// </summary>
public class ListRecipeBooksArgs
{
    /// <summary>
    /// Only include books if there key comes before this
    /// </summary>
    public long? BeforeBookKey { get; set; }

    /// <summary>
    /// Only include books whose key comes after this
    /// </summary>
    public long? AfterBookKey { get; set; }

    /// <summary>
    /// Ordering of books
    /// </summary>
    public RecordOrdering? ResultOrder { get; set; }

    /// <summary>
    /// Max number of books to return
    /// </summary>
    public required int ResultCount { get; set; }
}
