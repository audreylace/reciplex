namespace Reciplex.Server.Database.RecipeBooksDomain;

public class UpdateRecipeBookArgs
{
    public required string Name { get; set; }
    public required string ShortDescription { get; set; }
    public required string ConcurrencyTag { get; set; }
}
