using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reciplex.Server.Abstractions;

namespace Reciplex.Server.Database;

/// <summary>
/// Runs logic specific to SQLite3 on startup
/// </summary>
/// <param name="app">the application</param>
public class Sqlite3BeforeAppStartup(
    IServiceProvider serviceProvider,
    IOptions<SqliteApplicationDbContextOptions> options
) : IRunBeforeAppStartup
{
    /// <inheritdoc />
    public async Task RunBeforeStartupAsync(CancellationToken ct)
    {
        if (!options.Value.Enable)
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        ApplicationDbContext context = services.GetRequiredService<ApplicationDbContext>();

        using var connection = context.Database.GetDbConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL;";
        command.ExecuteScalar();

        if (options.Value.EnableMigrations)
        {
            // Applies any pending migrations and creates the database if it doesn't exist
            await context.Database.MigrateAsync(ct);
        }
    }
};
