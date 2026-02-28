namespace Reciplex.Server.RecipeServices.Recipes;

public class UpdateRecipeArgs
{
    public required string Name { get; init; }
    public required string ShortDescription { get; init; }
    public required string Details { get; init; }
}
