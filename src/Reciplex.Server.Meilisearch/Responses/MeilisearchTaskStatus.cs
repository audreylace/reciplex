namespace Reciplex.Server.Meilisearch.Responses;

public enum MeilisearchTaskStatus
{
    Enqueued = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    Canceled = 5,
}
