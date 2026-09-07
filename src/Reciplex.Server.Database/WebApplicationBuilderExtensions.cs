using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.DeletionWorker;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.RecipesDomain;
using Reciplex.Server.Database.SearchExporter;
using Reciplex.Server.Database.Strategies;
using Reciplex.Server.Database.UsersDomain;

namespace Reciplex.Server.Database;

/// <summary>
/// Configures application DB access for the program
/// </summary>
public static partial class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Add services providing application domain logic over the database
    /// </summary>
    /// <param name="builder">the app builder</param>
    /// <returns><paramref name="builder"/> with services registered</returns>
    public static WebApplicationBuilder AddApplicationDbSupportServices(
        this WebApplicationBuilder builder
    )
    {
        builder.Services.AddScoped<IUsersService, UsersService>();
        builder.Services.AddScoped<IRecipesService, RecipesService>();
        builder.Services.AddScoped<IRecipeBooksService, RecipeBooksService>();
        builder.Services.AddRandomNumberGeneratorConcurrencyTagProvider();
        builder.Services.AddHostedService<DeletionWorkerService>();
        builder.Services.AddSingleton<DeletionWorkerServiceMetrics>();
        builder.Services.AddSingleton<RepeatedDatabaseActionStrategy>();
        builder.Services.AddSingleton<SearchIndexCreationStrategy>();

        return builder;
    }

    /// <summary>
    /// Adds application DB context to the application
    /// </summary>
    /// <param name="builder">the app builder</param>
    /// <returns><paramref name="builder"/> with sqlite3 configured per the loaded configuration</returns>
    public static WebApplicationBuilder AddSqlite3ApplicationDbContext(
        this WebApplicationBuilder builder
    )
    {
        SqliteApplicationDbContextOptions configOptions = new();
        builder
            .Configuration.GetSection(SqliteApplicationDbContextOptions.SectionPath)
            .Bind(configOptions);

        if (!configOptions.Enable)
        {
            return builder;
        }

        if (string.IsNullOrWhiteSpace(configOptions.DatabaseConnection))
        {
            throw new InvalidOperationException(
                "DatabaseConnection string must be supplied when Sqlite3 is enabled"
            );
        }

        builder.Services.AddSingleton<IRunBeforeAppStartup, Sqlite3BeforeAppStartup>();
        builder.Services.Configure<SqliteApplicationDbContextOptions>(
            builder.Configuration.GetSection(SqliteApplicationDbContextOptions.SectionPath)
        );

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                configOptions.DatabaseConnection,
                b =>
                {
                    b.MigrationsAssembly(typeof(WebApplicationBuilderExtensions).Assembly.FullName);
                }
            )
        );

        builder.AddApplicationDbSupportServices();

        return builder;
    }
}
