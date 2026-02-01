using Reciplex.Server.Host.Services.StringIdInterop;
using Reciplex.Server.UserServices;

namespace Reciplex.Server.Host.UserKeyUtils;

/// <summary>
/// Implements <see cref="IStringUserKeyInterop"/> using <see cref="IStringIdInterop"/>
/// </summary>
/// <param name="stringIdInterop">Provider for marshalling longs to and from strings</param>
public class StringUserKeyInterop(IStringIdInterop stringIdInterop) : IStringUserKeyInterop
{
    public UserKey? AsKey(string s)
    {
        long? parsedLong = stringIdInterop.AsLong(s);
        return parsedLong is null ? null : new UserKey(parsedLong.Value);
    }

    public string AsString(UserKey recipeKey)
    {
        return stringIdInterop.AsString(recipeKey.SurrogateKey);
    }
}
