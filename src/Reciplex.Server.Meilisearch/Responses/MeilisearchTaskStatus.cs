using System.Text.Json.Serialization;

namespace Reciplex.Server.Meilisearch.Responses;

/// <summary>
/// Meilisearch task status <c>enum{string}</c>
/// </summary>
/// <remarks>
/// Status of the task. Possible values are enqueued, processing, succeeded, failed, and canceled.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MeilisearchTaskStatus
{
    /// <summary>
    /// Task is waiting processing
    /// </summary>
    [JsonStringEnumMemberName("enqueued")]
    Enqueued = 1,

    /// <summary>
    /// Task is processing
    /// </summary>
    [JsonStringEnumMemberName("processing")]
    Processing = 2,

    /// <summary>
    /// Task has succeeded
    /// </summary>
    [JsonStringEnumMemberName("succeeded")]
    Succeeded = 3,

    /// <summary>
    /// Task has failed
    /// </summary>
    [JsonStringEnumMemberName("failed")]
    Failed = 4,

    /// <summary>
    /// Task has been cancelled
    /// </summary>
    [JsonStringEnumMemberName("canceled")]
    Canceled = 5,
}
