namespace Reciplex.Server.Host;

/// <summary>
/// registers application error handler
/// </summary>
public static class ProductionErrorHandlerApplicationExtension
{
    /// <summary>
    /// registers the default application error handler which redirects
    /// the user to `/app-error` on failure.
    /// </summary>
    /// <param name="application">the web application</param>
    /// <returns><paramref name="application"/></returns>
    public static WebApplication UseRedirectOnError(this WebApplication application)
    {
        application.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                context.Response.Redirect("/app-error");
                await Task.CompletedTask;
            });
        });
        return application;
    }
}
