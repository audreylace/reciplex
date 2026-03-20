using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

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
                var options = requestHttpContext.RequestServices.GetService<
                    IOptions<AuthenticationFlowOptions>
                >();

                if (options?.Value.EnableAuthMocking == true)
                {
                    ClaimsIdentity claimsIdentity = new(identity ?? "Debug");
                    claimsIdentity.AddClaim(new(JwtRegisteredClaimNames.Iss, "DEBUG"));
                    claimsIdentity.AddClaim(new(JwtRegisteredClaimNames.Sub, userId ?? "1"));
                    requestHttpContext.User = new(claimsIdentity);
                }
                return nextPipelineHandler(requestHttpContext);
            }
        );

        return app;
    }

    public static WebApplicationBuilder AddAuthenticationFlowOptions(
        this WebApplicationBuilder applicationBuilder
    )
    {
        applicationBuilder.Services.Configure<AuthenticationFlowOptions>(
            applicationBuilder.Configuration.GetSection(AuthenticationFlowOptions.SectionPath)
        );
        return applicationBuilder;
    }

    public static WebApplicationBuilder AddOpenIdConnect(
        this WebApplicationBuilder applicationBuilder
    )
    {
        // add OIDC settings
        OpenIdConnectOptions? config = applicationBuilder
            .Configuration.GetSection(OpenIdConnectOptions.SectionPath)
            .Get<OpenIdConnectOptions>();

        if (config?.Enabled != true)
        {
            return applicationBuilder;
        }

        if (applicationBuilder.Environment.IsProduction() && config.DisableHttps)
        {
            throw new InvalidOperationException(
                "HTTPs connection to OIDC authority can not be disabled in production"
            );
        }

        // add OIDC
        applicationBuilder
            .AddAuthenticationFlowOptions()
            .Services.AddAuthentication(options =>
            {
                // stores auth data in cookie
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                // default schema maps to 401
                options.DefaultChallengeScheme = DefaultChallengeHandler.Schema;
            })
            .AddScheme<AuthenticationSchemeOptions, DefaultChallengeHandler>(
                DefaultChallengeHandler.Schema,
                null
            )
            .AddCookie(o =>
            {
                // 401/403 for redirects as this is an API server
                o.Events.OnRedirectToLogin = (context) =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };

                o.Events.OnRedirectToAccessDenied = (context) =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            })
            .AddOpenIdConnect(options =>
            {
                options.Authority = config.Authority;
                options.ClientId = config.ClientId;
                options.ClientSecret = config.ClientSecret;

                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ResponseType = OpenIdConnectResponseType.Code;

                options.GetClaimsFromUserInfoEndpoint = true;
                options.MapInboundClaims = true;
                options.SignedOutCallbackPath = "/api/v1/oidc/sign-out";
                options.CallbackPath = "/api/v1/oidc/sign-in";
                options.RequireHttpsMetadata = !config.DisableHttps;
                options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                options.Events.OnTokenValidated = (
                    context =>
                    {
                        // you can --
                        // - add custom claims via this hook
                        // - merge old identity with new incoming one to allow account linking
                        return Task.CompletedTask;
                    }
                );
            });

        return applicationBuilder;
    }
}
