using Reciplex.Server.RecipeServices.Recipes;

namespace Reciplex.Server.Host.Utils.RecipeKeyUtils;

/// <summary>
/// Interop services for marshalling <see cref="RecipeKey"/> to and from strings
/// </summary>
public interface IStringRecipeKeyInterop
{
    /// <summary>
    /// Marshals the <see cref="RecipeKey"/> into a string for HTTP transport
    /// </summary>
    /// <param name="recipeKey">The key to marshal</param>
    /// <returns>the <see cref="RecipeKey"/> as a string for http transport</returns>
    public string AsString(RecipeKey recipeKey);

    /// <summary>
    /// Marshals the <see cref="RecipeKey"/> from a string
    /// </summary>
    /// <param name="s">The string value of the key</param>
    /// <returns>The parsed recipe key or null on failure</returns>
    public RecipeKey? AsKey(string s);
}
