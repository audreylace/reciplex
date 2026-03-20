namespace Reciplex.Server.Database.RecipeBooksDomain;

public class CreateRecipeBookArgs
{
    public required string Name { get; set; }
    public required string ShortDescription { get; set; }
}
