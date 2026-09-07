using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.SearchExporter;

static partial class SearchExporterServiceLogger
{
    [LoggerMessage(LogLevel.Error, "Unhandled exception deleting empty record search entries")]
    public static partial void Error_UnhandledExceptionDeletingEmptySearchRecords(
        this ILogger<RecipeSearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception exporting a set of records to the search index")]
    public static partial void Error_ExportingBatchToSearchIndex(
        this ILogger<RecipeSearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception exporting {RecipeFk} to the search index")]
    public static partial void Error_ExportingRecipeToSearchIndex(
        this ILogger<RecipeSearchExporterService> logger,
        long RecipeFk,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Failed to increment attempt counter for search index entry with primary id {RecipeFk} at search version {SearchVersion}"
    )]
    public static partial void Error_IncrementingExtractionAttemptCounter(
        this ILogger<RecipeSearchExporterService> logger,
        long RecipeFk,
        long SearchVersion,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception running deletion operation on search index")]
    public static partial void Error_DeleteFromSearchIndex(
        this ILogger<RecipeSearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception running initial search entry creation")]
    public static partial void Error_RunningSearchEntryCreation(
        this ILogger<RecipeSearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception exporting to search index")]
    public static partial void Error_UnhandledExceptionWhenSearchExporting(
        this ILogger<RecipeSearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Exception marking record {RecipeFk} as extracted at search version {SearchVersion}"
    )]
    public static partial void Error_UnhandledExceptionWhenMarkingRecordAsExtracted(
        this ILogger<RecipeSearchExporterService> logger,
        long RecipeFk,
        long SearchVersion,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception deleting recipe search records in the stuck status.")]
    public static partial void Error_UnhandledExceptionDeletingStuckEntries(
        this ILogger<RecipeSearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception breaking lease entries.")]
    public static partial void Error_UnhandledExceptionBreakingLeaseEntries(
        this ILogger<RecipeSearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception deleting recipe {RecipeId} search record")]
    public static partial void Error_UnhandledExceptionDeletingRecipeSearchRecord(
        this ILogger<RecipeSearchExporterService> logger,
        long RecipeId,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Exception trying to mark recipe {RecipeId} search record export as failed"
    )]
    public static partial void Error_FailedToMarkDeleteAsFailed(
        this ILogger<RecipeSearchExporterService> logger,
        long RecipeId,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Exception trying to delete a batch of recipes from the search index"
    )]
    public static partial void Error_DeleteBatchFailed(
        this ILogger<RecipeSearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Exception trying to deleting recipe {RecipeId} from the search index"
    )]
    public static partial void Error_DeleteRecipeFromSearchIndexFailed(
        this ILogger<RecipeSearchExporterService> logger,
        long RecipeId,
        Exception ex
    );
}
