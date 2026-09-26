using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.SearchBackgroundTasks;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class NewRecipeExporterSearchBackgroundTaskLogger
{
    [LoggerMessage(LogLevel.Error, "Got an exception running the new recipe export loop.")]
    public static partial void Error_NewRecipeExportLoopFailed(
        this ILogger<NewRecipeExporterSearchBackgroundTask> logger,
        Exception ex
    );
}
