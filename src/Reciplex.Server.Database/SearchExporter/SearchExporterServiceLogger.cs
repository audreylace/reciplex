using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.SearchExporter;

static partial class SearchExporterServiceLogger
{
    [LoggerMessage(LogLevel.Error, "Unhandled exception deleting empty record search entries")]
    public static partial void Error_UnhandledExceptionDeletingEmptySearchRecords(
        this ILogger<SearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception exporting a set of records to the search index")]
    public static partial void Error_ExportingBatchToSearchIndex(
        this ILogger<SearchExporterService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception exporting {RecipeFk} to the search index")]
    public static partial void Error_ExportingRecipeToSearchIndex(
        this ILogger<SearchExporterService> logger,
        long RecipeFk,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Failed to increment attempt counter for search index entry with primary id {RecipeFk} at search version {SearchVersion}"
    )]
    public static partial void Error_IncrementingExtractionAttemptCounter(
        this ILogger<SearchExporterService> logger,
        long RecipeFk,
        long SearchVersion,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Exception running deletion operation on search index")]
    public static partial void Error_DeleteFromSearchIndex(
        this ILogger<SearchExporterService> logger,
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

    [LoggerMessage(
        LogLevel.Error,
        "Exception marking record {RecipeFk} as extracted at search version {SearchVersion}"
    )]
    public static partial void Error_UnhandledExceptionWhenMarkingRecordAsExtracted(
        this ILogger<SearchExporterService> logger,
        long RecipeFk,
        long SearchVersion,
        Exception ex
    );
}
