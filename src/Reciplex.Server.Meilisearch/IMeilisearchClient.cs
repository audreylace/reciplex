using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
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
    /// <param name="indexUid">the index id</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>info about the index if found or null otherwise</returns>
    public Task<GetIndexResponse?> GetIndexAsync(string indexUid, CancellationToken ct);
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
    public Task<MeilisearchTaskResponse> DeleteDocumentsAsync<T>(
        string indexUid,
        IEnumerable<T> documentIds,
        CancellationToken ct
    );

    public Task<TaskStatusResponse?> GetTaskStatusAsync(long taskId, CancellationToken ct);

    public Task<TaskStatusResponse?> WaitForTaskCompletionAsync(
        long taskId,
        CancellationToken ct,
        TimeSpan? pollFrequency = null
    );

    public Task<MeilisearchTaskResponse> ReplaceFilterableAttributesAsync(
        string indexUid,
        IEnumerable<string> attributes,
        CancellationToken ct
    );

    public Task<MeiliFilterAttributes?> GetFilterableAttributesAsync(
        string indexUid,
        CancellationToken ct
    );

    /// <summary>
    /// Runs a complex search via the post endpoint
    /// </summary>
    /// <param name="indexUid">the index id</param>
    /// <param name="args">search args</param>
    /// <param name="ct">async cancellation token</param>
    /// <typeparam name="TIndexDocument">shape of the index document</typeparam>
    /// <returns>the search result or null if the index is not found</returns>
    public Task<SearchQueryResponse<TIndexDocument>?> SearchByPostAsync<TIndexDocument>(
        string indexUid,
        SearchByPostArgs args,
        CancellationToken ct
    );
}
