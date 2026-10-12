using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.RecipesDomain;
using Reciplex.Server.Database.SearchIndexTrackingDomain;
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
        // user services
        builder.Services.AddSingleton<IUsersService, UsersService>();
        builder.Services.AddSingleton<IUserDeletionSignal, UserDeletionSignal>();
        builder.Services.AddHostedService<UserDeletionHostedService>();

        // recipes
        builder.Services.AddSingleton<IRecipesService, RecipesService>();
        builder.Services.AddHostedService<RecipeDeletionHostedService>();

        builder.Services.AddRandomNumberGeneratorConcurrencyTagProvider();

        // recipe books
        builder.Services.AddSingleton<IRecipeBooksService, RecipeBooksService>();
        builder.Services.AddHostedService<RecipeBookDeletionWorkerHostedService>();

        // search index
        builder.Services.AddSingleton<
            IRecipeSearchExportStatusRepository,
            RecipeSearchExportStatusRepository
        >();

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

        builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
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
