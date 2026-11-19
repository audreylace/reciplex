using Microsoft.Extensions.Logging;
using Reciplex.Server.SearchExporter.SearchBackgroundTasks;

namespace Reciplex.Server.SearchExporter.Loggers;

static partial class ChangedRecipeExporterSearchBackgroundTaskLogger
{
    [LoggerMessage(LogLevel.Error, "Got an exception running the change recipe export loop.")]
    public static partial void Error_ChangedRecipeExportLoopFailed(
        this ILogger<ChangedRecipeExporterSearchBackgroundTask> logger,
        Exception ex
    );
}
