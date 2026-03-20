namespace Reciplex.Server.Database.RecipesDomain;

public class UpdateRecipeArgs
{
    public required string Name { get; init; }
    public required string ShortDescription { get; init; }
    public required string Details { get; init; }
}
