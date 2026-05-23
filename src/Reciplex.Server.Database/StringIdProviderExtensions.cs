using Reciplex.Server.Abstractions.StringIdProvider;

namespace Reciplex.Server.Database;

/// <summary>
/// Extensions on <see cref="IStringIdProvider"/>
/// </summary>
internal static class StringIdProviderExtensions
{
    /// <summary>
    /// Tries to parse a string key
    /// </summary>
    /// <param name="stringIdProvider">the provider</param>
    /// <param name="key">string key</param>
    /// <param name="long">parsed long</param>
    /// <returns>true if the key could be parsed</returns>
    internal static bool TryParseStringKey(
        this IStringIdProvider stringIdProvider,
        string key,
        out long @long
    )
    {
        @long = default;
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }
        long? parsedValue = stringIdProvider.AsLong(key);

        if (parsedValue is not null)
        {
            @long = parsedValue.Value;
            return true;
        }
        return false;
    }
}
