using System.Security.Claims;

namespace Reciplex.Server.Host.AccessControl;

/// <summary>
/// <see cref="WebApplication"/> extensions for configuring authentication and authorization services in the pipeline
/// </summary>
public static class AccessControlWebApplicationExtensions
{
    /// <summary>
    /// Adds debug mocking throwing if <c>app.Environment.IsDevelopment()</c> returns false
    /// </summary>
    /// <param name="app">application to modify</param>
    /// <param name="identity">The mocked identity</param>
    /// <param name="userId">The mocked user id</param>
    /// <param name="claim">The claim where <paramref name="userId"/> will be stored inside <paramref name="identity"/></param>
    /// <returns><paramref name="app"/></returns>
    /// <exception cref="InvalidOperationException">Thrown when <c>WebApplication.Environment.IsDevelopment()</c> returns false</exception>
    public static WebApplication UseUserDebugMocking(
        this WebApplication app,
        string? identity = null,
        string? claim = null,
        string? userId = null
    )
    {
        if (!app.Environment.IsDevelopment()) // can only run in production
        {
            throw new InvalidOperationException(
                $"{nameof(UseUserDebugMocking)} can only be called when the environment is development"
            );
        }

        app.Use(nextPipelineHandler =>
            requestHttpContext =>
            {
                ClaimsIdentity claimsIdentity = new(identity ?? "Debug");
                claimsIdentity.AddClaim(new(claim ?? "sub", userId ?? "1"));
                requestHttpContext.User = new(claimsIdentity);
                return nextPipelineHandler(requestHttpContext);
            }
        );

        return app;
    }
}
