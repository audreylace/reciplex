using System.Text.Json.Serialization;

namespace Reciplex.Server.Meilisearch.Responses;

public class UpsertDocumentsResponse
{
    public required long TaskUid { get; init; }
    public required MeilisearchTaskStatus Status { get; init; }
    public string? IndexUid { get; init; }
    public DateTimeOffset EnqueuedAt { get; init; }
}

public class CreateIndexResponse
{
    public long TaskUid { get; init; }
    public required string IndexUid { get; init; }
    public required MeilisearchTaskStatus Status { get; init; }

    [JsonPropertyName("type")]
    public required MeilisearchTaskKind TaskKind { get; init; }
    public required DateTimeOffset EnqueuedAt { get; init; }
}
