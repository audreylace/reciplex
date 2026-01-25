namespace Reciplex.Server.Host.Models.Responses;

public class SingleRecipeBookResponseJson
{
    public required RecipeBookJson RecipeBook { get; init; }
    public required UserJson Owner { get; init; }
}
