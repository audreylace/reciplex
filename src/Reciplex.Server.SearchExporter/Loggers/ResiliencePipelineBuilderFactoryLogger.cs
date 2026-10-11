using Microsoft.Extensions.Logging;
using Reciplex.Server.SearchExporter;

namespace Reciplex.Server.SearchExporter.Loggers;

static partial class ResiliencePipelineBuilderFactoryLogger
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
