using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Reciplex.Server.Host.AccessControl;

/// <summary>
/// <see cref="WebApplication"/> extensions for configuring authentication and authorization services in the pipeline
/// </summary>
public static class AccessControlWebApplicationExtensions
{
#if DEBUG
    /// <summary>
    /// Adds debug mocking throwing if <c>app.Environment.IsDevelopment()</c> returns false
    /// </summary>
    /// <param name="app">application to modify</param>
    /// <param name="identity">The mocked identity</param>
    /// <param name="userId">The mocked user id</param>
    /// <param name="claim">The claim where <paramref name="userId"/> will be stored inside <paramref name="identity"/></param>
    /// <returns><paramref name="app"/></returns>
    /// <exception cref="InvalidOperationException">Thrown when <c>WebApplication.Environment.IsDevelopment()</c> returns false</exception>
    public static WebApplication UseUserDebugMocking(this WebApplication app, string? userId = null)
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
                    IOptions<DebugIdentityMockingOptions>
                >();

                if (options?.Value.Enable == true)
                {
                    ClaimsIdentity claimsIdentity = new("Debug");
                    claimsIdentity.AddClaim(new(JwtRegisteredClaimNames.Iss, "DEBUG"));
                    claimsIdentity.AddClaim(
                        new(JwtRegisteredClaimNames.Sub, options.Value.UserId ?? "1")
                    );
                    requestHttpContext.User = new(claimsIdentity);
                }
                return nextPipelineHandler(requestHttpContext);
            }
        );

        return app;
    }

    /// <summary>
    /// Debug only identity mocking
    /// </summary>
    public class DebugIdentityMockingOptions
    {
        /// <summary>
        /// Section path
        /// </summary>
        public const string SectionPath = "Reciplex:Debug:IdentityMocking";

        /// <summary>
        /// In debug builds setting this to true binds
        /// fake credentials to each request
        /// </summary>
        public bool Enable { get; set; }

        public string? UserId { get; set; }
    }

    public static WebApplicationBuilder AddAuthenticationDebugOptions(
        this WebApplicationBuilder applicationBuilder
    )
    {
        applicationBuilder.Services.Configure<DebugIdentityMockingOptions>(
            applicationBuilder.Configuration.GetSection(DebugIdentityMockingOptions.SectionPath)
        );
        return applicationBuilder;
    }
#endif

    public static WebApplicationBuilder AddOpenIdConnect(
        this WebApplicationBuilder applicationBuilder
    )
    {
        // add OIDC settings
        OpenIdConnectOptions? connectOptions = applicationBuilder
            .Configuration.GetSection(OpenIdConnectOptions.SectionPath)
            .Get<OpenIdConnectOptions>();

        if (connectOptions?.Enable != true)
        {
            return applicationBuilder;
        }

        if (string.IsNullOrWhiteSpace(connectOptions.Authority))
        {
            throw new InvalidOperationException(
                "Open ID Connect authentication is enabled by a URI to the authority is missing."
            );
        }

        OpenIdConnectSecretsOptions? secretsOptions =
            applicationBuilder
                .Configuration.GetSection(OpenIdConnectSecretsOptions.SectionPath)
                .Get<OpenIdConnectSecretsOptions>()
            ?? throw new InvalidOperationException("Secrets for OpenIdConnect must be provided");

#if DEBUG
        if (applicationBuilder.Environment.IsProduction() && connectOptions.InsecureDisableHttps)
        {
            throw new InvalidOperationException(
                "HTTPs connection to OIDC authority can not be disabled in production"
            );
        }
#endif

        // add OIDC
        applicationBuilder
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
                options.Authority = connectOptions.Authority;
                options.ClientId = secretsOptions.ClientId;
                options.ClientSecret = secretsOptions.ClientSecret;

                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ResponseType = OpenIdConnectResponseType.Code;

                options.GetClaimsFromUserInfoEndpoint = true;
                options.MapInboundClaims = true;
                options.SignedOutCallbackPath = "/api/v1/oidc/sign-out";
                options.CallbackPath = "/api/v1/oidc/sign-in";

#if DEBUG
                options.RequireHttpsMetadata = !connectOptions.InsecureDisableHttps;
#endif

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
