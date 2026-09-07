using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Reciplex.Server.Meilisearch.Responses;

namespace Reciplex.Server.Meilisearch;

public class MeilisearchClient(HttpClient httpClient) : IMeilisearchClient
{
    record class CreateIndexRequest(string Uid, string PrimaryKey);

    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<MeilisearchTaskResponse> CreateIndexAsync(
        string uuid,
        string primaryKey,
        CancellationToken ct
    )
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "indexes",
            new CreateIndexRequest(uuid, primaryKey),
            _options,
            ct
        );

        return await DecodeTaskResponse(response, ct);
    }

    private static async Task<MeilisearchTaskResponse> DecodeTaskResponse(
        HttpResponseMessage response,
        CancellationToken ct
    )
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
        {
            return await ReadJsonOrThrow<MeilisearchTaskResponse>(response, ct);
        }
        throw CreateUnhandledStatusCodeException(response);
    }

    private static MeilisearchApiException CreateUnhandledStatusCodeException(
        HttpResponseMessage response
    )
    {
        // throw on failure
        response.EnsureSuccessStatusCode();

        // if we are here we got something that is a success but we don't know how to handle
        return new MeilisearchApiException(
            $"got back unexpected status code {response.StatusCode}"
        );
    }

    private static void ThrowIfIndexNotFound(HttpResponseMessage response, string indexUid)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new SearchIndexDoesNotExistException(indexUid);
        }
    }

    private static async Task<T> ReadJsonOrThrow<T>(
        HttpResponseMessage response,
        CancellationToken ct
    )
    {
        return await response.Content.ReadFromJsonAsync<T>(_options, ct)
            ?? throw new MeilisearchApiException("response unexpectedly decoded to null");
    }

    public async Task<MeilisearchTaskResponse> DeleteDocumentsAsync(
        string indexUid,
        IEnumerable<string> documentIds,
        CancellationToken ct
    )
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"indexes/{Uri.EscapeDataString(indexUid)}/documents/delete-batch",
            documentIds.ToArray(),
            _options,
            ct
        );

        ThrowIfIndexNotFound(response, indexUid);

        return await DecodeTaskResponse(response, ct);
    }

    public async Task<GetIndexResponse?> GetIndexAsync(string indexUid, CancellationToken ct)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(
            $"indexes/{Uri.EscapeDataString(indexUid)}",
            ct
        );

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await ReadJsonOrThrow<GetIndexResponse>(response, ct);
    }

    public async Task<TaskStatusResponse?> GetTaskStatusAsync(long taskId, CancellationToken ct)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(
            $"tasks/{taskId.ToString(CultureInfo.InvariantCulture)}",
            ct
        );

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await ReadJsonOrThrow<TaskStatusResponse>(response, ct);
    }

    public async Task<MeilisearchTaskResponse> UpsertDocumentsAsync<T>(
        string indexUid,
        IEnumerable<T> documents,
        CancellationToken ct
    )
        where T : class
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"indexes/{Uri.EscapeDataString(indexUid)}/documents",
            documents.ToArray(),
            _options,
            ct
        );

        ThrowIfIndexNotFound(response, indexUid);

        return await DecodeTaskResponse(response, ct);
    }

    public async Task<TaskStatusResponse?> WaitForTaskCompletionAsync(
        long taskId,
        CancellationToken ct,
        TimeSpan? pollFrequency = null
    )
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var response = await GetTaskStatusAsync(taskId, ct);
            if (response == null || response.IsTerminal())
            {
                return response;
            }
            await Task.Delay(pollFrequency ?? TimeSpan.FromMilliseconds(100), ct);
        }
    }

    public async Task<TaskStatusResponse?> UpsertDocumentsAndWaitAsync<T>(
        string indexUid,
        IEnumerable<T> documents,
        CancellationToken ct
    )
        where T : class
    {
        MeilisearchTaskResponse result = await UpsertDocumentsAsync(indexUid, documents, ct);

        TaskStatusResponse? taskStatus = await WaitForTaskCompletionAsync(result.TaskUid, ct);

        return taskStatus;
    }
}
