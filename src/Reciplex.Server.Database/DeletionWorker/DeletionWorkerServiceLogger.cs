using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.DeletionWorker;

/// <summary>
/// Log messages for <see cref="DeletionWorkerService"/>
/// </summary>
internal static partial class DeletionWorkerServiceLogger
{
    /// <summary>
    /// Logged from <see cref="RunUntilCompletionWithDelay"/> when an error is hit
    /// </summary>
    /// <param name="logger">logger to use</param>
    /// <param name="databaseObjectName">the database object</param>
    /// <param name="ex">the encountered error</param>
    [LoggerMessage(
        "Database operation failed with exception for database object '{databaseObjectName}'",
        Level = LogLevel.Error
    )]
    public static partial void RunUntilCompletionWithDelayError(
        this ILogger<DeletionWorkerService> logger,
        string databaseObjectName,
        Exception ex
    );

    /// <summary>
    /// Message logging when no cleanup work done
    /// </summary>
    /// <param name="logger">logger to use</param>
    /// <param name="minutes">the time duration</param>
    [LoggerMessage(
        "No cleanup work done. Will run again in {minutes} minutes.",
        Level = LogLevel.Information
    )]
    public static partial void NoCleanupWorkDone(
        this ILogger<DeletionWorkerService> logger,
        double minutes
    );

    /// <summary>
    /// Message logging when cleanup work was done
    /// </summary>
    /// <param name="logger">logger to use</param>
    /// <param name="minutes">the time duration</param>
    [LoggerMessage(
        "Cleanup work done. Will run again in {minutes} minutes.",
        Level = LogLevel.Information
    )]
    public static partial void CleanupWorkDone(
        this ILogger<DeletionWorkerService> logger,
        double minutes
    );
}
