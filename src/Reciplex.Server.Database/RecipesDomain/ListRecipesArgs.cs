namespace Reciplex.Server.Database.RecipesDomain;

public class ListRecipesArgs
{
    public string? BeforeRecipeId { get; set; }
    public string? AfterRecipeId { get; set; }
    public RecordOrdering? ResultOrder { get; set; }
    public required int ResultCount { get; set; }
    public string? RecipeBookId { get; set; }
}
