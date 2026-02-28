namespace Reciplex.Server.RecipeServices.Recipes;

public class ListRecipesArgs
{
    public RecipeKey? BeforeRecipeId { get; set; }
    public RecipeKey? AfterRecipeId { get; set; }
    public ListRecipesOrdering? ResultOrder { get; set; }
    public required int ResultCount { get; set; }
}
