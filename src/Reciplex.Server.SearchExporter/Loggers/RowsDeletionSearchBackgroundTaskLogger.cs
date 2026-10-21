using Microsoft.Extensions.Logging;
using Reciplex.Server.SearchExporter.SearchBackgroundTasks;

namespace Reciplex.Server.SearchExporter.Loggers;

static partial class RowsDeletionSearchBackgroundTaskLogger
{
    [LoggerMessage(
        LogLevel.Error,
        "Exception caught while deleting search tracking rows from the database"
    )]
    public static partial void Error_DeleteRowsFromIndexFailed(
        this ILogger<RowsDeletionSearchBackgroundTask> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Exception caught deleting row with id '{RowId}' from the search index"
    )]
    public static partial void Error_DeletingRowFromIndexFailed(
        this ILogger<RowsDeletionSearchBackgroundTask> logger,
        long RowId,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Deleting search index document failed for row with id '{RowId}'"
    )]
    public static partial void Error_DeletingRowFromIndexFailed(
        this ILogger<RowsDeletionSearchBackgroundTask> logger,
        long RowId
    );

    [LoggerMessage(LogLevel.Error, "Caught exception from lease renewal task")]
    public static partial void Error_RenewTaskFailed(
        this ILogger<RowsDeletionSearchBackgroundTask> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Failed to clear one or more search row leases")]
    public static partial void Error_FailedToClearLeases(
        this ILogger<RowsDeletionSearchBackgroundTask> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Failed to clear leases and delete one or more search row entries after purging their data from the search index"
    )]
    public static partial void Error_FailedToDeleteRows(
        this ILogger<RowsDeletionSearchBackgroundTask> logger,
        Exception ex
    );

    [LoggerMessage(
        LogLevel.Error,
        "Failed to increment failure counter and clear leases for {RowId} after attempting to purging their data from the search index"
    )]
    public static partial void Error_FailedToIncrementRowsFailureCounter(
        this ILogger<RowsDeletionSearchBackgroundTask> logger,
        long RowId,
        Exception ex
    );
}
