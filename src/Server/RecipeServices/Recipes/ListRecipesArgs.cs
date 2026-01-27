namespace Reciplex.Server.RecipeServices.Recipes;

public class ListRecipesArgs
{
    public string? BeforeRecipeId { get; set; }
    public string? AfterRecipeId { get; set; }
    public ListRecipesOrdering? ResultOrder { get; set; }
    public required int ResultCount { get; set; }
    public string? BookId { get; set; }
}
