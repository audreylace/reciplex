using Microsoft.Extensions.Logging;
using Reciplex.Server.Meilisearch.Responses;

namespace Reciplex.Server.Database.SearchExporter;

/// <summary>
/// Logger for <see cref="SearchIndexCreationStrategy" />
/// </summary>
static partial class SearchIndexCreationStrategyLogger
{
    [LoggerMessage(
        LogLevel.Error,
        "Exception running index creation for index {Index} with {PrimaryKey}"
    )]
    public static partial void Error_IndexCreationFailedWithException(
        this ILogger<SearchIndexCreationStrategy> logger,
        string Index,
        string PrimaryKey,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Index creation task for index {IndexName} with id {TaskUid} failed : status was {TaskStatus}"
    )]
    public static partial void Error_IndexCreationTaskFailed(
        this ILogger<SearchIndexCreationStrategy> logger,
        string IndexName,
        long? TaskUid,
        MeilisearchTaskStatus? TaskStatus
    );
}
