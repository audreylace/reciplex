using Reciplex.Server.UserServices;

namespace Reciplex.Server.Host.UserKeyUtils;

/// <summary>
/// Interop services for marshalling <see cref="UserKey"/> to and from strings
/// </summary>
public interface IStringUserKeyInterop
{
    /// <summary>
    /// Marshals the <see cref="UserKey"/> into a string for HTTP transport
    /// </summary>
    /// <param name="recipeKey">The key to marshal</param>
    /// <returns>the <see cref="UserKey"/> as a string for http transport</returns>
    public string AsString(UserKey recipeKey);

    /// <summary>
    /// Marshals the <see cref="UserKey"/> from a string
    /// </summary>
    /// <param name="s">The string value of the key</param>
    /// <returns>The parsed user key or null on failure</returns>
    public UserKey? AsKey(string s);
}
