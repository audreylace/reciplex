using Microsoft.Extensions.Logging;
using Reciplex.Server.Meilisearch.Responses;

namespace Reciplex.Server.Database.SearchExporter;

static partial class SearchExporterServiceLogger
{
    [LoggerMessage(LogLevel.Error, "Unhandled exception deleting empty record search entries")]
    public static partial void Error_UnhandledExceptionDeletingEmptySearchRecords(
        this ILogger<SearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception exporting a set of records to the search index")]
    public static partial void Error_ExportToSearchIndex(
        this ILogger<SearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception deleting a batch of records from the search index")]
    public static partial void Error_DeletingBatchOfRecordsFromSearchIndex(
        this ILogger<SearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Extraction task with id {TaskUid} failed : status was {TaskStatus}"
    )]
    public static partial void Error_ExtractionTaskFailed(
        this ILogger<SearchExporterService> logger,
        long? TaskUid,
        MeilisearchTaskStatus? TaskStatus
    );

    [LoggerMessage(
        LogLevel.Error,
        "Deletion task with id {TaskUid} failed : status was {TaskStatus}"
    )]
    public static partial void Error_DeletionTaskFailed(
        this ILogger<SearchExporterService> logger,
        long? TaskUid,
        MeilisearchTaskStatus? TaskStatus
    );

    [LoggerMessage(
        LogLevel.Error,
        "Index creation task for index {IndexName} with id {TaskUid} failed : status was {TaskStatus}"
    )]
    public static partial void Error_IndexCreationTaskFailed(
        this ILogger<SearchExporterService> logger,
        string IndexName,
        long? TaskUid,
        MeilisearchTaskStatus? TaskStatus
    );

    [LoggerMessage(
        LogLevel.Error,
        "Failed to increment deletion attempt counter for search index entry with primary id {SearchIndexId} and recipe record {RecordId}"
    )]
    public static partial void Error_IncrementingFailedIndexDeletionCounter(
        this ILogger<SearchExporterService> logger,
        long SearchIndexId,
        long RecordId,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Failed to increment attempt counter for search index entry with primary id {SearchIndexId} and recipe record {RecordId}"
    )]
    public static partial void Error_IncrementingExtractionAttemptCounter(
        this ILogger<SearchExporterService> logger,
        long SearchIndexId,
        long RecordId,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception running deletion operation on search index")]
    public static partial void Error_DeleteFromSearchIndex(
        this ILogger<SearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Exception running index creation for index {Index} with {PrimaryKey}"
    )]
    public static partial void Error_IndexCreationFailedWithException(
        this ILogger<SearchExporterService> logger,
        string Index,
        string PrimaryKey,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception running initial search entry creation")]
    public static partial void Error_RunningSearchEntryCreation(
        this ILogger<SearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception exporting to search index")]
    public static partial void Error_UnhandledExceptionWhenSearchExporting(
        this ILogger<SearchExporterService> logger,
        Exception ex
    );
}
