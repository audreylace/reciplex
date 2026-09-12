using System.Text.Json.Serialization;

namespace Reciplex.Server.Meilisearch.Responses;

/// <summary>
/// Response sent back from an endpoint of Meilisearch. The
/// actual create operation happens in the batch processor meaning the task
/// as identified by <see cref="TaskUid"/> must be polled.
/// </summary>
public class MeilisearchTaskResponse
{
    /// <summary>
    /// The id of the task for polling
    /// </summary>
    public long TaskUid { get; init; }

    /// <summary>
    /// The id of te index if this task applies to an index
    /// </summary>
    public string? IndexUid { get; init; }
    public required MeilisearchTaskStatus Status { get; init; }

    [JsonPropertyName("type")]
    public required MeilisearchTaskKind TaskKind { get; init; }
    public required DateTimeOffset EnqueuedAt { get; init; }
}
