using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reciplex.Server.Database;

namespace Recipe.Database;

public class Sqlite3Migrator(WebApplication app)
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

            if (configOptions.EnableMigrations && configOptions.Enable)
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                // Applies any pending migrations and creates the database if it doesn't exist
                await context.Database.MigrateAsync(ct);
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Sqlite3Migrator>>();
            logger.LogMigrationError(ex);
            throw;
        }
    }
};
