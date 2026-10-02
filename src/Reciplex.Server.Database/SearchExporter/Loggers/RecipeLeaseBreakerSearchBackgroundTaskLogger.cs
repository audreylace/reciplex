using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.SearchBackgroundTasks;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class RecipeLeaseBreakerSearchBackgroundTaskLogger
{
    [LoggerMessage(LogLevel.Error, "Got an exception while breaking leases")]
    public static partial void Error_LeaseBreakingFailed(
        this ILogger<RecipeLeaseBreakerSearchBackgroundTask> logger,
        Exception ex
    );
}
