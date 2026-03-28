using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime;
using NodaTime.Text;

namespace Reciplex.Server.Host;

public class NodaInstantJsonConverter : JsonConverter<Instant>
{
    public override Instant Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        string s = reader.GetString() ?? throw new JsonException();
        var result = InstantPattern.General.Parse(s);
        return result.Value;
    }

    public override void Write(Utf8JsonWriter writer, Instant value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(
            value.ToString(InstantPattern.General.PatternText, CultureInfo.InvariantCulture)
        );
    }
}
