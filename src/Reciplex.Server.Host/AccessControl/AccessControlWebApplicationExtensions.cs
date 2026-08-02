using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Reciplex.Server.Host.Metrics;
using Reciplex.Server.Host.Options;

namespace Reciplex.Server.Host.AccessControl;

/// <summary>
/// <see cref="WebApplication"/> extensions for configuring authentication and authorization services in the pipeline
/// </summary>
public static class AccessControlWebApplicationExtensions
{
    /// <summary>
    /// The named back channel client for OIDC
    /// </summary>
    private const string OpenIdConnectClient = "OidcHttpBackchannel";

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
        ReciplexOpenIdConnectOptions? connectOptions = applicationBuilder
            .Configuration.GetSection(ReciplexOpenIdConnectOptions.SectionPath)
            .Get<ReciplexOpenIdConnectOptions>();

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

        ReciplexOpenIdConnectSecretsOptions? secretsOptions =
            applicationBuilder
                .Configuration.GetSection(ReciplexOpenIdConnectSecretsOptions.SectionPath)
                .Get<ReciplexOpenIdConnectSecretsOptions>()
            ?? throw new InvalidOperationException("Secrets for OpenIdConnect must be provided");

        applicationBuilder.Services.Configure<ReciplexOpenIdConnectOptions>(
            applicationBuilder.Configuration.GetSection(ReciplexOpenIdConnectOptions.SectionPath)
        );
        applicationBuilder.Services.Configure<ReciplexOpenIdConnectSecretsOptions>(
            applicationBuilder.Configuration.GetSection(
                ReciplexOpenIdConnectSecretsOptions.SectionPath
            )
        );

        applicationBuilder
            .Services.AddHttpClient(OpenIdConnectClient)
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

                if (connectOptions.InsecureAcceptAnyServerCertificate)
                {
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                // If you use a DelegatingHandler for route overriding, wrap it here:
                if (!string.IsNullOrWhiteSpace(connectOptions.BackChannelHostOverride))
                {
                    return new OidcOverrideBackChannelRoutingHandler(connectOptions, handler);
                }

                return handler;
            })
            .ConfigureAdditionalHttpMessageHandlers(
                (handlers, serviceProvider) =>
                {
                    handlers.Add(new HttpMetricsClientEnricherHandler("OpenIdConnect"));
                }
            );

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
            .AddOpenIdConnect();

        ConfigureOpenIdConnectOptions(applicationBuilder, connectOptions, secretsOptions);

        return applicationBuilder;
    }

    /// <summary>
    /// Configures <see cref="OpenIdConnectOptions" />
    /// </summary>
    /// <param name="applicationBuilder">the app builder</param>
    /// <param name="connectOptions">the oidc connect options</param>
    /// <param name="secretsOptions">the oidc secrets</param>
    private static void ConfigureOpenIdConnectOptions(
        WebApplicationBuilder applicationBuilder,
        ReciplexOpenIdConnectOptions connectOptions,
        ReciplexOpenIdConnectSecretsOptions secretsOptions
    )
    {
        applicationBuilder
            .Services.AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
            .Configure<
                IHttpClientFactory,
                IOptions<ReciplexOpenIdConnectOptions>,
                IOptions<ReciplexOpenIdConnectSecretsOptions>
            >(
                (options, httpFactory, rOptions, rSecrets) =>
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

                    options.Backchannel = httpFactory.CreateClient(OpenIdConnectClient);
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
                }
            );
    }
}
