using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.HostedServices;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipeSearchIndexSetupHostedServiceLogger
{
    [LoggerMessage(
        LogLevel.Error,
        "Exception creating or verifying the recipe search index is setup"
    )]
    public static partial void Error_IndexOperationFailed(
        this ILogger<RecipeSearchHostedBackgroundService> logger,
        Exception? ex
    );

    [LoggerMessage(LogLevel.Error, "Background export task threw")]
    public static partial void Error_BackgroundTaskFailed(
        this ILogger<RecipeSearchHostedBackgroundService> logger,
        Exception? ex
    );

    [LoggerMessage(LogLevel.Error, "Unhandled exception from main background task monitor")]
    public static partial void Error_BackgroundTaskMonitorFailed(
        this ILogger<RecipeSearchHostedBackgroundService> logger,
        Exception? ex
    );
}
