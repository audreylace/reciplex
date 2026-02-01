using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reciplex.Server.Host.Models.PagingUtils;

public class NavigationDirectionJsonConverter : JsonConverter<NavigationDirection>
{
    public override NavigationDirection? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return NavigationDirection.Parse(reader.GetString() ?? throw new JsonException(), null);
    }

    public override void Write(
        Utf8JsonWriter writer,
        NavigationDirection value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStringValue(NavigationDirection.ToQueryStringParameterValue(value.Direction));
    }
}
