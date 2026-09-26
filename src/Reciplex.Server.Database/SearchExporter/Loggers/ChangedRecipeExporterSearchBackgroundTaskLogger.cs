using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.SearchBackgroundTasks;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class ChangedRecipeExporterSearchBackgroundTaskLogger
{
    [LoggerMessage(
        LogLevel.Error,
        "Got an exception waiting for recipe change notifications in the changed recipe export loop."
    )]
    public static partial void Error_ChangedRecipeExportLoopChannelFailed(
        this ILogger<ChangedRecipeExporterSearchBackgroundTask> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Error, "Got an exception running the change recipe export loop.")]
    public static partial void Error_ChangedRecipeExportLoopFailed(
        this ILogger<ChangedRecipeExporterSearchBackgroundTask> logger,
        Exception ex
    );
}
