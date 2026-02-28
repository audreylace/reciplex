using Reciplex.Server.Host.Models.RecipeBook;
using Reciplex.Server.Host.Models.User;

namespace Reciplex.Server.Host.Models.Recipe;

public class SingleRecipeResponseJson
{
    public required RecipeJson Recipe { get; init; }
    public required RecipeBookJson RecipeBook { get; init; }
    public required UserJson RecipeBookOwner { get; init; }
}
