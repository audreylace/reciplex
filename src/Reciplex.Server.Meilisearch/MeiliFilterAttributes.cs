using System.Text.Json.Serialization;

namespace Reciplex.Server.Meilisearch;

[JsonConverter(typeof(MeiliFilterAttributesConverter))]
public class MeiliFilterAttributes
{
    /// <summary>
    /// List of properties with all filtering enabled
    /// </summary>
    public required List<string> Properties { get; init; }

    /// <summary>
    /// Set of attributes with enhanced configuration
    /// </summary>
    public required List<MeiliFilterAttributeConfig> MeiliFilterAttributeConfigs { get; init; }
}
