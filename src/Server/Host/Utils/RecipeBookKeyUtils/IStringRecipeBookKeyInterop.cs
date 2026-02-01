using Reciplex.Server.RecipeServices.RecipeBooks;

namespace Reciplex.Server.Host.Utils.RecipeBookKeyUtils;

/// <summary>
/// Interop services for marshalling <see cref="RecipeBookKey"/> to and from strings
/// </summary>
public interface IStringRecipeBookKeyInterop
{
    /// <summary>
    /// Marshals the <see cref="RecipeBookKey"/> into a string for HTTP transport
    /// </summary>
    /// <param name="recipeBookKey">The key to marshal</param>
    /// <returns>the <see cref="RecipeBookKey"/> as a string for http transport</returns>
    public string AsString(RecipeBookKey recipeBookKey);

    /// <summary>
    /// Marshals the <see cref="RecipeBookKey"/> from a string
    /// </summary>
    /// <param name="s">The string value of the key</param>
    /// <returns>The parsed recipe book key or null on failure</returns>
    public RecipeBookKey? AsKey(string s);
}
