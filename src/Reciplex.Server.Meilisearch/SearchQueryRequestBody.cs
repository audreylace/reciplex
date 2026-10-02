using System.Text.Json.Serialization;

namespace Reciplex.Server.Meilisearch;

class SearchQueryRequestBody
{
    [JsonPropertyName("q")]
    public required string SearchString { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Filter { get; init; }
}
