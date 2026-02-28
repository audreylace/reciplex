namespace Reciplex.Server.RecipeServices.RecipeBooks;

public class UpdateRecipeBookDetailsArgs
{
    public required string Name { get; set; }

    public required string ShortDescription { get; set; }
    public required string ConcurrencyTag { get; set; }
}
