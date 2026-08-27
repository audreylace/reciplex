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

    public void EnsureSuccess()
    {
        if (Status == MeilisearchTaskStatus.Succeeded)
        {
            return;
        }
        throw new MeilisearchApiException("task does not represent success");
    }

    public bool IsTerminal()
    {
        return Status == MeilisearchTaskStatus.Succeeded
            || Status == MeilisearchTaskStatus.Canceled
            || Status == MeilisearchTaskStatus.Failed;
    }
}
