using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database;

public class Sqlite3Startup(WebApplication app)
{
    public async Task MigrateAsync(CancellationToken ct)
    {
        using var scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;
        try
        {
            SqliteApplicationDbContextOptions configOptions = new();
            app.Configuration.GetSection(SqliteApplicationDbContextOptions.SectionPath)
                .Bind(configOptions);

            if (!configOptions.Enable)
            {
                return;
            }

            var context = services.GetRequiredService<ApplicationDbContext>();

            using var connection = context.Database.GetDbConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA journal_mode=WAL;";
            command.ExecuteScalar();

            if (configOptions.EnableMigrations)
            {
                // Applies any pending migrations and creates the database if it doesn't exist
                await context.Database.MigrateAsync(ct);
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Sqlite3Startup>>();
            logger.LogMigrationError(ex);
            throw;
        }
    }
};
