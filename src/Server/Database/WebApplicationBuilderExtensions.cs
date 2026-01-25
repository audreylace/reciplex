using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Recipe.Database;

/// <summary>
/// Configures application DB access for the program
/// </summary>
public static partial class WebApplicationBuilderExtensions
{
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
        return builder;
    }
}
