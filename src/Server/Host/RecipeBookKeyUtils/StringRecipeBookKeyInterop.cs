using Reciplex.Server.Host.RecipeKeyUtils;
using Reciplex.Server.Host.Services.StringIdInterop;
using Reciplex.Server.RecipeServices.RecipeBooks;

namespace Reciplex.Server.Host.RecipeBookKeyUtils;

/// <summary>
/// Implements <see cref="IStringRecipeKeyInterop"/> using <see cref="IStringIdInterop"/>
/// </summary>
/// <param name="stringIdInterop">Provider for marshalling longs to and from strings</param>
public class StringRecipeBookKeyInterop(IStringIdInterop stringIdInterop)
    : IStringRecipeBookKeyInterop
{
    public RecipeBookKey? AsKey(string s)
    {
        long? parsedLong = stringIdInterop.AsLong(s);
        return parsedLong is null ? null : new RecipeBookKey(parsedLong.Value);
    }

    public string AsString(RecipeBookKey recipeKey)
    {
        return stringIdInterop.AsString(recipeKey.SurrogateKey);
    }
}
