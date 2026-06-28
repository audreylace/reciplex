using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.RecipesDomain;
using Reciplex.Server.Database.UsersDomain;

namespace Reciplex.Server.Database;

/// <summary>
/// Configures application DB access for the program
/// </summary>
public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddApplicationDbSupportServices(
        this WebApplicationBuilder builder
    )
    {
        builder.Services.AddScoped<IUsersService, UsersService>();
        builder.Services.AddScoped<IRecipesService, RecipesService>();
        builder.Services.AddScoped<IRecipeBooksService, RecipeBooksService>();
        builder.Services.AddRandomNumberGeneratorConcurrencyTagProvider();

        return builder;
    }

    /// <summary>
    /// Adds application DB context to the application
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
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
