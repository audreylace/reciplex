using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database;

public static partial class Sqlite3StartupLogger
{
    [LoggerMessage(LogLevel.Error, "An error occurred while migrating the database.")]
    public static partial void LogMigrationError(this ILogger<Sqlite3Startup> logger, Exception ex);
}
