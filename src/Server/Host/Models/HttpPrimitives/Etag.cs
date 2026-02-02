using System.Diagnostics.CodeAnalysis;

namespace Reciplex.Server.Host.Models.HttpPrimitives;

public readonly record struct EtagValue(string Value) : IParsable<EtagValue>
{
    public static EtagValue Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, null, out EtagValue result))
        {
            return result;
        }

        throw new ArgumentException("bad navigation value", nameof(s));
    }

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

        if (s[0] != '\"' || s.Last() != '\"' || s.Length <= 2)
        {
            return false;
        }

        result = new(s[1..^1]);

        return true;
    }
}
