using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class SearchIndexExporterScopeLogger
{
    [LoggerMessage(LogLevel.Error, "Exception monitoring background tasks")]
    public static partial void Error_BackgroundTaskMonitorFailed(
        this ILogger<SearchIndexExporterScope> logger,
        Exception? ex
    );

    [LoggerMessage(LogLevel.Error, "Background export task threw")]
    public static partial void Error_BackgroundTaskFailed(
        this ILogger<SearchIndexExporterScope> logger,
        Exception? ex
    );
}
