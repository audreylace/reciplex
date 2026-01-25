using System.Diagnostics.CodeAnalysis;

namespace Reciplex.Server.Host.Models.Requests;

public class NavigationDirection : IParsable<NavigationDirection>
{
    public enum DirectionValue
    {
        Backwards,
        Forwards,
    }

    public required DirectionValue Direction { get; init; }

    public static NavigationDirection Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, null, out NavigationDirection? result))
        {
            return result;
        }

        throw new ArgumentException("bad navigation value", nameof(s));
    }

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out NavigationDirection result
    )
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        if (s.Equals("forward", StringComparison.InvariantCultureIgnoreCase))
        {
            result = new() { Direction = DirectionValue.Forwards };
            return true;
        }

        if (s.Equals("backward", StringComparison.InvariantCultureIgnoreCase))
        {
            result = new() { Direction = DirectionValue.Backwards };
            return true;
        }

        return false;
    }
}
