using System.Text.Json;
using System.Text.Json.Serialization;
using Reciplex.Server.UserServices;

namespace Reciplex.Server.Host.Utils.UserKeyUtils;

public class UserKeyJsonConverter(IStringUserKeyInterop userKeyInterop) : JsonConverter<UserKey>
{
    public override UserKey Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        string s = reader.GetString() ?? throw new JsonException();
        return userKeyInterop.AsKey(s) ?? throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, UserKey value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(userKeyInterop.AsString(value));
    }
}
