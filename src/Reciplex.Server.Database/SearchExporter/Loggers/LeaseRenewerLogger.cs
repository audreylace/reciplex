using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.SearchExporter.Loggers;

static partial class LeaseRenewerLogger
{
    [LoggerMessage(
        LogLevel.Warning,
        "Got an exception renewing a lease. Retry attempt is {Attempt}."
    )]
    public static partial void Warning_LeaseRenewAttemptFailed(
        this ILogger<LeaseRenewer> logger,
        int Attempt,
        Exception? ex
    );

    [LoggerMessage(LogLevel.Error, "Got an exception renewing a lease. At max attempt {Attempt}.")]
    public static partial void Error_MaxLeaseRenewAttempt(
        this ILogger<LeaseRenewer> logger,
        int Attempt,
        Exception? ex
    );
}
