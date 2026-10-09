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
    /// Logged from <see cref="RunUntilCompletionWithDelay"/> when an error is hit
    /// </summary>
    /// <param name="logger">logger to use</param>
    /// <param name="databaseObjectName">the database object</param>
    /// <param name="ex">the encountered error</param>
    [LoggerMessage("Caught exception in delete loop", Level = LogLevel.Error)]
    public static partial void Error_ExceptionInMainLoop(
        this ILogger<DeletionWorkerService> logger,
        Exception ex
    );
}
