using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.UsersDomain;

static partial class UsersQueuedForDeletionHostedServiceLogger
{
    [LoggerMessage(LogLevel.Error, "Got an exception running the delete user loop")]
    public static partial void Error_BackgroundLoopFailure(
        this ILogger<UsersQueuedForDeletionHostedService> logger,
        Exception ex
    );

    [LoggerMessage(LogLevel.Information, "Starting background user deletion loop")]
    public static partial void Info_LoopStarting(
        this ILogger<UsersQueuedForDeletionHostedService> logger
    );

    [LoggerMessage(LogLevel.Information, "Stopping background user deletion loop")]
    public static partial void Info_LoopStopping(
        this ILogger<UsersQueuedForDeletionHostedService> logger
    );
}
