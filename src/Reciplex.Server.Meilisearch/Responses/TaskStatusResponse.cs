using System.Text.Json.Serialization;

namespace Reciplex.Server.Meilisearch.Responses;

public class TaskStatusResponse
{
    public required long Uid { get; init; }
    public required MeilisearchTaskStatus Status { get; init; }
    public string? IndexUid { get; init; }
    public DateTimeOffset EnqueuedAt { get; init; }

    [JsonPropertyName("type")]
    public MeilisearchTaskKind Kind { get; init; }
}
