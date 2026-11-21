using Microsoft.Extensions.Logging;
using Reciplex.Server.SearchExporter.HostedServices;

namespace Reciplex.Server.SearchExporter.Loggers;

static partial class PurgeMarkedForDeletionHostedServiceLogger
{
    [LoggerMessage(LogLevel.Information, "Deleted search entries purge worker started")]
    public static partial void Info_Starting(
        this ILogger<PurgeMarkedForDeletionHostedService> logger
    );

    [LoggerMessage(LogLevel.Information, "Deleted search entries purge worker has stopped")]
    public static partial void Info_Stopping(
        this ILogger<PurgeMarkedForDeletionHostedService> logger
    );

    [LoggerMessage(
        LogLevel.Error,
        "Deleted search entries purge worker encountered an error. Pausing for 5 seconds."
    )]
    public static partial void Error_GotException(
        this ILogger<PurgeMarkedForDeletionHostedService> logger,
        Exception ex
    );
}
