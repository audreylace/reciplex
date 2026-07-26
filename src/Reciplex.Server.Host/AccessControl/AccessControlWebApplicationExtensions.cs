using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Reciplex.Server.Host.Options;

namespace Reciplex.Server.Host.AccessControl;

/// <summary>
/// <see cref="WebApplication"/> extensions for configuring authentication and authorization services in the pipeline
/// </summary>
public static class AccessControlWebApplicationExtensions
{
    /// <summary>
    /// Adds and Configures OIDC for the application
    /// </summary>
    /// <param name="applicationBuilder">the app to configure</param>
    /// <returns><paramref name="applicationBuilder"/> with OIDC services added and configured</returns>
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

                o.Cookie.HttpOnly = true;
                o.Cookie.IsEssential = true;
                o.Cookie.SameSite = SameSiteMode.Strict; // guard against some types of CSRF attacks
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always; // cookie over https only

                o.SlidingExpiration = true;
                o.ExpireTimeSpan = TimeSpan.FromDays(14);
            })
            .AddOpenIdConnect(options =>
            {
                options.Authority = connectOptions.Authority;
                options.ClientId = secretsOptions.ClientId;
                options.ClientSecret = secretsOptions.ClientSecret;

                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ResponseType = OpenIdConnectResponseType.Code;

                options.ClaimActions.Remove("iss"); // keep iss
                options.GetClaimsFromUserInfoEndpoint = true;
                options.MapInboundClaims = false; // don't mutate our claims
                options.SignedOutCallbackPath = "/api/v1/oidc/sign-out";
                options.CallbackPath = "/api/v1/oidc/sign-in";
                options.RequireHttpsMetadata = !connectOptions.InsecureDisableHttps;
                options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;

                // OIDC is just to identify and authenticate user, after that,
                // this application takes control of the session lifetime.
                options.UseTokenLifetime = false;
                options.SaveTokens = false;

                if (connectOptions.InsecureAcceptAnyServerCertificate)
                {
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                    };
                }

                if (!string.IsNullOrWhiteSpace(connectOptions.BackChannelHostOverride))
                {
                    options.BackchannelHttpHandler = new OidcOverrideBackChannelRoutingHandler(
                        connectOptions,
                        options.BackchannelHttpHandler ?? new HttpClientHandler()
                    );
                }

                options.Events.OnTicketReceived = context =>
                {
                    context.Properties ??= new();
                    context.Properties.IsPersistent = true;
                    context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14);

                    return Task.CompletedTask;
                };

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
