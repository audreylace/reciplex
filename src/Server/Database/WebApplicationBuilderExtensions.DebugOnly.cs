using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Recipe.Database;

/// <summary>
/// Configures application DB access for the program
/// </summary>
public static partial class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the application in development mode
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddApplicationDbContextForDebug(
        this WebApplicationBuilder builder
    )
    {
        if (!builder.Environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "this method can only be called when running in development mode"
            );
        }
        string dbString = $"Data Source=bin/{Guid.NewGuid()}.db";
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(dbString));
        builder.Services.AddHostedService<ConfigureSqliteDbForDevelopment>();
        return builder;
    }
}
