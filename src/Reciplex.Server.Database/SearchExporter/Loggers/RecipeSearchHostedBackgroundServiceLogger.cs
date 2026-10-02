using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.HostedServices;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipeSearchHostedBackgroundServiceLogger
{
    [LoggerMessage(LogLevel.Error, "Search index export task threw an exception")]
    public static partial void Error_SearchIndexExportThrew(
        this ILogger<RecipeSearchHostedBackgroundService> logger,
        Exception? ex
    );
}
