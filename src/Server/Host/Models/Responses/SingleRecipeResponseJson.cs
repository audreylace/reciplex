namespace Reciplex.Server.Host.Models.Responses;

public class SingleRecipeResponseJson
{
    public required RecipeJson Recipe { get; init; }
    public required RecipeBookJson RecipeBook { get; init; }
    public required UserJson RecipeBookOwner { get; init; }
}
