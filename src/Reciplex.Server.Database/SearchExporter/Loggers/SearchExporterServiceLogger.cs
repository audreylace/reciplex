using Microsoft.Extensions.Logging;
using Reciplex.Server.Meilisearch.Responses;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class SearchExporterServiceLogger
{
    [LoggerMessage(LogLevel.Error, "Unhandled exception deleting empty record search entries")]
    public static partial void Error_UnhandledExceptionDeletingEmptySearchRecords(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception exporting a set of records to the search index")]
    public static partial void Error_ExportingBatchToSearchIndex(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception exporting {RecipeFk} to the search index")]
    public static partial void Error_ExportingRecipeToSearchIndex(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        long RecipeFk,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Failed to increment attempt counter for search index entry with primary id {RecipeFk} at search version {SearchVersion}"
    )]
    public static partial void Error_IncrementingExtractionAttemptCounter(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        long RecipeFk,
        long SearchVersion,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception running deletion operation on search index")]
    public static partial void Error_DeleteFromSearchIndex(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception running initial search entry creation")]
    public static partial void Error_RunningSearchEntryCreation(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception exporting to search index")]
    public static partial void Error_UnhandledExceptionWhenSearchExporting(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Exception marking record {RecipeFk} as extracted at search version {SearchVersion}"
    )]
    public static partial void Error_UnhandledExceptionWhenMarkingRecordAsExtracted(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        long RecipeFk,
        long SearchVersion,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception deleting recipe search records in the stuck status.")]
    public static partial void Error_UnhandledExceptionDeletingStuckEntries(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception breaking lease entries.")]
    public static partial void Error_UnhandledExceptionBreakingLeaseEntries(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception deleting recipe {RecipeId} search record")]
    public static partial void Error_UnhandledExceptionDeletingRecipeSearchRecord(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        long RecipeId,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Exception trying to mark recipe {RecipeId} search record export as failed"
    )]
    public static partial void Error_FailedToMarkDeleteAsFailed(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        long RecipeId,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Exception trying to delete a batch of recipes from the search index"
    )]
    public static partial void Error_DeleteBatchFailed(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Exception trying to deleting recipe {RecipeId} from the search index"
    )]
    public static partial void Error_DeleteRecipeFromSearchIndexFailed(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        long RecipeId,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Exception running index creation for index {Index} with {PrimaryKey}"
    )]
    public static partial void Error_IndexCreationFailedWithException(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        string Index,
        string PrimaryKey,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Index creation task for index {IndexName} with id {TaskUid} failed : status was {TaskStatus}"
    )]
    public static partial void Error_IndexCreationTaskFailed(
        this ILogger<RecipeSearchBackgroundExporterService> logger,
        string IndexName,
        long? TaskUid,
        MeilisearchTaskStatus? TaskStatus
    );
}
