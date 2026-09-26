using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reciplex.Server.Meilisearch;

public class MeiliFilterAttributesConverter : JsonConverter<MeiliFilterAttributes>
{
    public override MeiliFilterAttributes Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected StartArray token, but found {reader.TokenType}.");
        }

        var properties = new List<string>();
        var configs = new List<MeiliFilterAttributeConfig>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return new MeiliFilterAttributes
                {
                    Properties = properties,
                    MeiliFilterAttributeConfigs = configs,
                };
            }

            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    properties.Add(reader.GetString()!);
                    break;

                case JsonTokenType.StartObject:
                    // Deserialize the object using standard options
                    var config = JsonSerializer.Deserialize<MeiliFilterAttributeConfig>(
                        ref reader,
                        options
                    );
                    if (config != null)
                    {
                        configs.Add(config);
                    }
                    break;

                default:
                    throw new JsonException(
                        $"Unexpected token type {reader.TokenType} encountered in filter attribute array."
                    );
            }
        }

        throw new JsonException("JSON payload ended prematurely before closing array bracket.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        MeiliFilterAttributes value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartArray();

        // Write simple strings
        if (value.Properties != null)
        {
            foreach (var prop in value.Properties)
            {
                writer.WriteStringValue(prop);
            }
        }

        // Write complex object configs
        if (value.MeiliFilterAttributeConfigs != null)
        {
            foreach (var config in value.MeiliFilterAttributeConfigs)
            {
                JsonSerializer.Serialize(writer, config, options);
            }
        }

        writer.WriteEndArray();
    }
}
