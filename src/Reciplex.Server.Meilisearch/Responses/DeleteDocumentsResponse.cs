namespace Reciplex.Server.Meilisearch.Responses;

public class DeleteDocumentsResponse
{
    public required long TaskUid { get; init; }
    public required MeilisearchTaskStatus Status { get; init; }
    public string? IndexUid { get; init; }
    public DateTimeOffset EnqueuedAt { get; init; }
}
