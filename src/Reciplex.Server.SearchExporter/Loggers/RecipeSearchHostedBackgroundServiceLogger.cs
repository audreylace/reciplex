using Microsoft.Extensions.Logging;
using Reciplex.Server.SearchExporter.HostedServices;

namespace Reciplex.Server.SearchExporter.Loggers;

static partial class RecipeSearchHostedBackgroundServiceLogger
{
    [LoggerMessage(LogLevel.Error, "Caught exception in main loop")]
    public static partial void Error_MainLoopCaughtException(
        this ILogger<RecipeSearchHostedBackgroundService> logger,
        Exception? ex
    );

    [LoggerMessage(LogLevel.Error, "Search index export task threw an exception")]
    public static partial void Error_SearchIndexExportTaskThrew(
        this ILogger<RecipeSearchHostedBackgroundService> logger,
        Exception? ex
    );

    [LoggerMessage(LogLevel.Error, "Search index monitor task threw an exception")]
    public static partial void Error_SearchIndexMonitorThrew(
        this ILogger<RecipeSearchHostedBackgroundService> logger,
        Exception? ex
    );

    [LoggerMessage(LogLevel.Information, "Search exporter service has started")]
    public static partial void Info_ServiceStarting(
        this ILogger<RecipeSearchHostedBackgroundService> logger
    );

    [LoggerMessage(LogLevel.Information, "Search exporter service has stopped")]
    public static partial void Info_ServiceStopping(
        this ILogger<RecipeSearchHostedBackgroundService> logger
    );
}
