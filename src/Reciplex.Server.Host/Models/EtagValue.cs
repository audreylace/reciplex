using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace Reciplex.Server.Host.Models;

/// <summary>
/// Etag parser for strong etag value. Does not support the weak prefix.
/// </summary>
/// <param name="Value">the unquoted etag value</param>
public readonly record struct EtagValue(string Value) : IParsable<EtagValue>
{
    /// <inheritdoc/>
    public static EtagValue Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, null, out EtagValue result))
        {
            return result;
        }

        throw new ArgumentException("bad navigation value", nameof(s));
    }

    /// <inheritdoc/>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out EtagValue result
    )
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        // make standard ASP.NET etag parser do the heavy lifting and validate the etag
        if (!EntityTagHeaderValue.TryParse(s, out EntityTagHeaderValue? etag))
        {
            return false;
        }

        // we don't support weak etags
        if (etag.IsWeak)
        {
            return false;
        }

        // slice off the quotes and de-escape the string
        string tag = etag.Tag;
        if (tag[0] != '\"' || tag.Last() != '\"' || tag.Length <= 2)
        {
            return false;
        }

        var slicedString = tag[1..^1];

        if (!slicedString.Any(tag => tag == '\''))
        {
            result = new(slicedString);
            return true;
        }

        result = new(slicedString.Replace("\\'", "'", StringComparison.InvariantCulture));
        return true;
    }

    /// <summary>
    /// Helper method for encoding a value into an ETAG format
    /// </summary>
    /// <param name="s">the value to encode</param>
    /// <returns>the encoded string</returns>
    public static string EncodeTag(string s)
    {
        if (s.Any(c => c == '\''))
        {
            return $"'{s.Replace("'", "\\'", StringComparison.InvariantCulture)}'";
        }
        return $"'{s}'";
    }
}
