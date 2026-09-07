using System.Diagnostics.CodeAnalysis;
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
    public Task<MeilisearchTaskResponse> CreateIndexAsync(
        string uuid,
        string primaryKey,
        CancellationToken ct
    );

    /// <summary>
    /// Adds or replaces documents in an index
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="indexUid">the index to operate on</param>
    /// <param name="documents">documents to create or replace</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>the result throwing on failure or if the index does not exist</returns>
    public Task<MeilisearchTaskResponse> UpsertDocumentsAsync<T>(
        string indexUid,
        IEnumerable<T> documents,
        CancellationToken ct
    )
        where T : class;

    public Task<TaskStatusResponse?> UpsertDocumentsAndWaitAsync<T>(
        string indexUid,
        IEnumerable<T> documents,
        CancellationToken ct
    )
        where T : class;

    /// <summary>
    /// Post a batch operation to delete a set of documents
    /// </summary>
    /// <param name="indexUid">the index to operate on</param>
    /// <param name="documentIds">documents to create or replace</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>the result throwing on failure or if the index does not exist</returns>
    public Task<MeilisearchTaskResponse> DeleteDocumentsAsync(
        string indexUid,
        IEnumerable<string> documentIds,
        CancellationToken ct
    );

    public Task<TaskStatusResponse?> GetTaskStatusAsync(long taskId, CancellationToken ct);

    public Task<TaskStatusResponse?> WaitForTaskCompletionAsync(
        long taskId,
        CancellationToken ct,
        TimeSpan? pollFrequency = null
    );
}
