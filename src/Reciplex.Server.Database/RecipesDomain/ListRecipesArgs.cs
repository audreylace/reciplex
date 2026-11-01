namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Args for listing recipes
/// </summary>
public class ListRecipesArgs
{
    /// <summary>
    /// Only recipes with an ID before this are included in the result set
    /// </summary>
    public long? BeforeRecipeId { get; set; }

    /// <summary>
    /// Only recipes with an ID after this are included in the result set
    /// </summary>
    public long? AfterRecipeId { get; set; }

    /// <summary>
    /// How to order the results
    /// </summary>
    public RecordOrdering? ResultOrder { get; set; }

    /// <summary>
    /// Limit for how many results to return
    /// </summary>
    public required int ResultCount { get; set; }

    /// <summary>
    /// optional book key to focus the results only to one book
    /// </summary>
    public long? RecipeBookId { get; set; }
}
