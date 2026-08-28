using System.Text.Json.Serialization;

namespace Reciplex.Server.Meilisearch.Responses;

public class CreateIndexResponse
{
    public long TaskUid { get; init; }
    public required string IndexUid { get; init; }
    public required MeilisearchTaskStatus Status { get; init; }

    [JsonPropertyName("type")]
    public required MeilisearchTaskKind TaskKind { get; init; }
    public required DateTimeOffset EnqueuedAt { get; init; }
}
