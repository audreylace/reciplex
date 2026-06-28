using Microsoft.Extensions.Logging;

namespace Recipe.Database;

public static partial class Sqlite3MigratorLogger
{
    [LoggerMessage(LogLevel.Error, "An error occurred while migrating the database.")]
    public static partial void LogMigrationError(
        this ILogger<Sqlite3Migrator> logger,
        Exception ex
    );
}
