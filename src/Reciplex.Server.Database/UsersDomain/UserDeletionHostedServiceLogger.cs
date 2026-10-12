using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.UsersDomain;

static partial class UserDeletionHostedServiceLogger
{
    [LoggerMessage(
        LogLevel.Error,
        "User deletion worker encountered an exception. Pausing work for 5 seconds."
    )]
    public static partial void Error_BackgroundLoopFailure(
        this ILogger<UserDeletionHostedService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Information, "User deletion worker has started")]
    public static partial void Info_LoopStarting(this ILogger<UserDeletionHostedService> logger);

    [LoggerMessage(LogLevel.Information, "User deletion worker has stopped")]
    public static partial void Info_LoopStopping(this ILogger<UserDeletionHostedService> logger);
}
