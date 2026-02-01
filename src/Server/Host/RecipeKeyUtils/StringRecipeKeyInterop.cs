using Reciplex.Server.Host.Services.StringIdInterop;
using Reciplex.Server.RecipeServices.Recipes;

namespace Reciplex.Server.Host.RecipeKeyUtils;

/// <summary>
/// Implements <see cref="IStringRecipeKeyInterop"/> using <see cref="IStringIdInterop"/>
/// </summary>
/// <param name="stringIdInterop">Provider for marshalling longs to and from strings</param>
public class StringRecipeKeyInterop(IStringIdInterop stringIdInterop) : IStringRecipeKeyInterop
{
    public RecipeKey? AsKey(string s)
    {
        long? parsedLong = stringIdInterop.AsLong(s);
        return parsedLong is null ? null : new RecipeKey(parsedLong.Value);
    }

    public string AsString(RecipeKey recipeKey)
    {
        return stringIdInterop.AsString(recipeKey.SurrogateKey);
    }
}
