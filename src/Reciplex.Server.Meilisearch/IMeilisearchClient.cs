using Reciplex.Server.Meilisearch.Responses;

namespace Reciplex.Server.Meilisearch;

/// <summary>
/// Wraps a http client for communicating with the remote Meilisearch server
/// </summary>
public interface IMeilisearchClient
{
    /// <summary>
    /// Gets an index by its name or returns null if the index does not exist.
    /// </summary>
    /// <param name="uid">the index id</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>info about the index if found or null otherwise</returns>
    public Task<GetIndexResponse?> GetIndexAsync(string uid, CancellationToken ct);

    /// <summary>
    /// Adds or replaces documents in an index
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="indexUid">the index to operate on</param>
    /// <param name="documents">documents to create or replace</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>the result or null if the index does not exist</returns>
    public Task<UpsertDocumentsResponse?> UpsertDocumentsAsync<T>(
        string indexUid,
        IEnumerable<T> documents,
        CancellationToken ct
    )
        where T : class;

    public Task<TaskStatusResponse> GetTaskStatus(long taskId, CancellationToken ct);
}
