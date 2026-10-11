using Microsoft.Extensions.Logging;
using Reciplex.Server.SearchExporter.HostedServices;

namespace Reciplex.Server.SearchExporter.Loggers;

static partial class RecipeSearchHostedBackgroundServiceLogger
{
    [LoggerMessage(LogLevel.Error, "Search index export task threw an exception")]
    public static partial void Error_SearchIndexExportThrew(
        this ILogger<RecipeSearchHostedBackgroundService> logger,
        Exception? ex
    );
}
