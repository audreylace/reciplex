namespace Reciplex.Server.RecipeServices.RecipeBooks;

public class CreateRecipeBookArgs
{
    public required string OwningUserId { get; set; }
    public required string Name { get; set; }
    public required string ShortDescription { get; set; }
}
