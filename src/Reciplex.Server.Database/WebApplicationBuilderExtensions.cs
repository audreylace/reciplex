using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Database;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.RecipesDomain;
using Reciplex.Server.Database.UsersDomain;

namespace Recipe.Database;

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
    public static WebApplicationBuilder AddApplicationDbContext(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("ApplicationDbContext"))
        );

        builder.AddApplicationDbSupportServices();

        return builder;
    }
}
