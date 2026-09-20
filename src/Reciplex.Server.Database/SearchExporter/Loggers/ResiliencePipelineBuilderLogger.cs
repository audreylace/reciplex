using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class ResiliencePipelineBuilderLogger
{
    [LoggerMessage(
        LogLevel.Warning,
        "Got an exception running a delete operation. Running retry attempt {Attempt}."
    )]
    public static partial void Warning_DeleteAttemptFailed(
        this ILogger<ResiliencePipelineBuilderFactory> logger,
        int Attempt,
        Exception? ex
    );
}
