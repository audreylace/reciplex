using Reciplex.Server.Host.Models.User;

namespace Reciplex.Server.Host.Models.RecipeBook;

public class SingleRecipeBookResponseJson
{
    public required RecipeBookJson RecipeBook { get; init; }
    public required UserJson Owner { get; init; }
}
