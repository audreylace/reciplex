using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using NodaTime;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Abstractions.StringIdProvider;
using Reciplex.Server.Database;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Options;
using Sqids;
using DataProtectionOptions = Reciplex.Server.Host.Options.DataProtectionOptions;

namespace Reciplex.Server.Host;

public class Program
{
    /// <summary>
    /// Main entry point for application
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        ParseAndApplyArgs(builder, args);

        ConfigureKestrel(builder);

        AddShortIds(builder);
        PostConfigureJsonOptions(builder);

        AddDataProtection(builder);

        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication();

        builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

        builder.Services.AddControllersWithViews();

        builder.Services.AddSingleton<IClock>(SystemClock.Instance);

        AddAntiforgery(builder);

        builder.AddSqlite3ApplicationDbContext();

        builder.Services.Configure<RoutingOptions>(
            builder.Configuration.GetSection(RoutingOptions.SectionPath)
        );

        builder.AddOpenIdConnect();

        ConfigureMetrics(builder);
        ConfigureRateLimiting(builder);

        var app = builder.Build();
        await RunStartupServices(app);

        app.MapHealthChecks("/api/healthz"); // map early in the pipeline

        UseContentSecurityHeaders(app);

        UseRedirectOnError(app);
        app.UseAuthentication();

        app.UseHsts();
        UseProxySupport(app);
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapGroup("/api/v1").MapControllers();
        app.MapStaticAssets();
        app.MapFallbackToFile("index.html");

        app.Run();
    }

    /// <summary>
    /// Customizes the kestrel server for the application
    /// </summary>
    /// <param name="builder">application builder</param>
    private static void ConfigureKestrel(WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false; // block server header since that leaks info about the runtime.
        });
    }

    /// <summary>
    /// Runs all <see cref="IRunBeforeAppStartup" /> registered in the application
    /// </summary>
    /// <param name="app">the application</param>
    private static async Task RunStartupServices(WebApplication app)
    {
        var toRunBeforeStart = app.Services.GetServices<IRunBeforeAppStartup>();
        foreach (IRunBeforeAppStartup service in toRunBeforeStart)
        {
            await service.RunBeforeStartupAsync(CancellationToken.None);
        }
    }

    /// <summary>
    /// Customizes json options for the application
    /// </summary>
    /// <param name="builder">the application builder</param>
    private static void PostConfigureJsonOptions(WebApplicationBuilder builder)
    {
        builder.Services.PostConfigure<JsonOptions>(jsonOptions =>
        {
            jsonOptions.SerializerOptions.DefaultIgnoreCondition = System
                .Text
                .Json
                .Serialization
                .JsonIgnoreCondition
                .WhenWritingNull;

            jsonOptions.SerializerOptions.Converters.Add(new NodaInstantJsonConverter());
        });
    }

    /// <summary>
    /// Configures the web pipeline's proxy settings
    /// </summary>
    /// <param name="app">the web pipeline whose proxy settings will be mutated</param>
    private static void UseProxySupport(WebApplication app)
    {
        RoutingOptions routingOptions = new();
        app.Configuration.Bind(RoutingOptions.SectionPath, routingOptions);

        if (routingOptions.InsecureTrustProxy)
        {
            var fwdOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor
                    | ForwardedHeaders.XForwardedProto
                    | ForwardedHeaders.XForwardedHost,
            };

            fwdOptions.KnownIPNetworks.Clear();
            fwdOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(fwdOptions);
        }
        else
        {
            app.UseHttpsRedirection();
        }
    }

    /// <summary>
    /// Adds content security headers to the application
    /// </summary>
    /// <param name="app">the web pipeline to add the headers to</param>
    private static void UseContentSecurityHeaders(WebApplication app)
    {
        app.Use(
            async (context, next) =>
            {
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append(
                    "Referrer-Policy",
                    "strict-origin-when-cross-origin"
                );
                context.Response.Headers.Append(
                    "Permissions-Policy",
                    "geolocation=(), camera=(), microphone=()"
                );

                await next();
            }
        );
    }

    /// <summary>
    /// Adds and configures antiforgery services
    /// </summary>
    /// <param name="builder">the application builder</param>
    private static void AddAntiforgery(WebApplicationBuilder builder)
    {
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-XSRF-TOKEN";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
    }

    /// <summary>
    /// Adds asp.net data protection to the application
    /// </summary>
    /// <param name="builder">app builder</param>
    private static void AddDataProtection(WebApplicationBuilder builder)
    {
        DataProtectionOptions? configOptions = builder
            .Configuration.GetSection(DataProtectionOptions.SectionPath)
            .Get<DataProtectionOptions>();

        IDataProtectionBuilder dpBuilder = builder.Services.AddDataProtection();

        if (!string.IsNullOrWhiteSpace(configOptions?.KeyStorageDirectory))
        {
            dpBuilder = dpBuilder.PersistKeysToFileSystem(
                new DirectoryInfo(configOptions.KeyStorageDirectory)
            );
        }

        if (
            !string.IsNullOrWhiteSpace(configOptions?.EncryptionCertificate)
            && !string.IsNullOrWhiteSpace(configOptions?.EncryptionPrivateKey)
        )
        {
            X509Certificate2 x509Cert = X509Certificate2.CreateFromPemFile(
                configOptions.EncryptionCertificate,
                configOptions.EncryptionPrivateKey
            );

            dpBuilder.ProtectKeysWithCertificate(x509Cert);
        }
        else if (!(configOptions?.InsecureDisableEncryption ?? false))
        {
            throw new InvalidOperationException(
                "no encryption certificate provided to protect data protect keys. If this is intentional then disable encryption."
            );
        }
    }

    /// <summary>
    /// Adds short id mapping to the container
    /// </summary>
    /// <param name="builder">app builder</param>
    /// <returns><paramref name="builder"/> with short id services added</returns>
    private static void AddShortIds(WebApplicationBuilder builder)
    {
        builder.Services.Configure<AppShortIdOptions>(
            builder.Configuration.GetSection(AppShortIdOptions.SectionPath)
        );
        builder.Services.AddSingleton<IStringIdProvider, SquidsStringIdProvider>();
        builder.Services.AddSingleton(provider =>
        {
            IOptions<AppShortIdOptions> options = provider.GetRequiredService<
                IOptions<AppShortIdOptions>
            >();

            SqidsOptions sqidsOptions = new();
            sqidsOptions.MinLength = options.Value.MinLength ?? sqidsOptions.MinLength;
            sqidsOptions.Alphabet = options.Value.Alphabet ?? sqidsOptions.Alphabet;

            if (options.Value.Banned is not null)
            {
                foreach (string word in options.Value.Banned)
                {
                    sqidsOptions.BlockList.Add(word);
                }
            }

            return new SqidsEncoder<long>(sqidsOptions);
        });
    }

    /// <summary>
    /// registers the default application error handler which redirects
    /// the user to `/app-error` on failure.
    /// </summary>
    /// <param name="application">the web application</param>
    /// <returns><paramref name="application"/></returns>
    private static void UseRedirectOnError(WebApplication application)
    {
        application.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                context.Response.Redirect("/app-error");
                await Task.CompletedTask;
            });
        });
    }

    /// <summary>
    /// Configures metric exportation to the victoria metrics instance
    /// </summary>
    /// <param name="builder">the web application builder for the application from which metrics will be exported</param>
    /// <exception cref="InvalidOperationException">thrown if metric exporting is enabled but no path is specified</exception>
    private static void ConfigureMetrics(WebApplicationBuilder builder)
    {
        AppMetricsOptions appMetricsOptions = new();
        builder.Configuration.GetSection(AppMetricsOptions.SectionPath).Bind(appMetricsOptions);

        if (!appMetricsOptions.Enable)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(appMetricsOptions.ExportUri))
        {
            throw new InvalidOperationException(
                "must specify the path to the victoria metric server"
            );
        }

        builder
            .Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                if (!string.IsNullOrWhiteSpace(appMetricsOptions.ServiceName))
                {
                    resource.AddService(appMetricsOptions.ServiceName);
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter("Reciplex.*")
                    .AddOtlpExporter(options =>
                    {
                        // Pointing to VictoriaMetrics OTLP ingest endpoint
                        options.Endpoint = new Uri(appMetricsOptions.ExportUri);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    });
            });
    }

    private static void ConfigureRateLimiting(WebApplicationBuilder builder)
    {
        AppRateLimitingOptions appRateLimiting = new();
        builder.Configuration.GetSection(AppRateLimitingOptions.SectionPath).Bind(appRateLimiting);

        builder.Services.AddRateLimiter(options =>
        {
            var authenticatedLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext =>
                {
                    var credentials = httpContext.OpenIdConnectCredentials();
                    if (credentials != null && appRateLimiting.Enable)
                    {
                        return RateLimitPartition.GetSlidingWindowLimiter(
                            partitionKey: $"user_{credentials.Value.Authority}_{credentials.Value.Subject}",
                            factory: _ => new SlidingWindowRateLimiterOptions
                            {
                                PermitLimit = appRateLimiting.AuthUserMaxRequests,
                                Window = TimeSpan.FromSeconds(
                                    appRateLimiting.AuthUserWindowSeconds
                                ),
                                SegmentsPerWindow = appRateLimiting.AuthUserWindowSegments,
                                QueueLimit = 0,
                            }
                        );
                    }

                    return RateLimitPartition.GetNoLimiter("authenticated_bypass");
                }
            );
            var globalAnonLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext =>
                {
                    var credentials = httpContext.OpenIdConnectCredentials();
                    if (credentials == null && appRateLimiting.Enable)
                    {
                        return RateLimitPartition.GetTokenBucketLimiter(
                            partitionKey: "global_anon",
                            factory: _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = appRateLimiting.GlobalBucketMaxTokens,
                                TokensPerPeriod = appRateLimiting.GlobalBucketReplenishRate,
                                ReplenishmentPeriod = TimeSpan.FromSeconds(
                                    appRateLimiting.GlobalBucketReplenishPeriodSeconds
                                ),
                                AutoReplenishment = true,
                                QueueLimit = 0,
                            }
                        );
                    }

                    return RateLimitPartition.GetNoLimiter("anon_bypass");
                }
            );

            var perIpAnonLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var credentials = httpContext.OpenIdConnectCredentials();
                if (credentials == null && appRateLimiting.Enable)
                {
                    string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: $"anon_ip_{ip}",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = appRateLimiting.PerIpMaxRequests,
                            Window = TimeSpan.FromSeconds(appRateLimiting.PerIpWindowSeconds),
                            QueueLimit = 0,
                        }
                    );
                }

                return RateLimitPartition.GetNoLimiter("anon_bypass");
            });

            // Chain all three limiters together into GlobalLimiter
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                authenticatedLimiter,
                globalAnonLimiter,
                perIpAnonLimiter
            );

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Too many requests.", token);
            };
        });
    }

    /// <summary>
    /// Parses arguments and applies them to the builder
    /// </summary>
    /// <param name="args">Incoming program arguments</param>
    /// <param name="builder">The builder to apply the arguments to</param>
    /// <remarks>This method will exit the application on parse failure. It assumes control over the whole program.</remarks>
    private static WebApplicationBuilder ParseAndApplyArgs(
        WebApplicationBuilder builder,
        string[] args
    )
    {
        bool eatArg = false;
        bool seenConfig = false;

        if (!args.Any(arg => arg == "--noArgs"))
        {
            // Add custom config paths from command line
            for (int i = 0; i < args.Length; i++)
            {
                if (eatArg)
                {
                    eatArg = false;
                    continue;
                }

                if (args[i] == "-R")
                {
                    if (seenConfig)
                    {
                        Console.WriteLine("-R must come before -C, -c, or -E arguments");
                        Environment.Exit(-1);
                    }

                    builder.Configuration.Sources.Clear();
                    continue;
                }

                // Optional config, little c
                if (args[i] == "-c" && i + 1 < args.Length)
                {
                    seenConfig = true;
                    string additionalConfigPath = args[i + 1];

                    builder.Configuration.AddJsonFile(
                        additionalConfigPath,
                        optional: true,
                        reloadOnChange: true
                    );
                    eatArg = true;
                    continue;
                }

                // Mandatory config, big C
                if (args[i] == "-C" && i + 1 < args.Length)
                {
                    string additionalConfigPath = args[i + 1];
                    seenConfig = true;
                    builder.Configuration.AddJsonFile(
                        additionalConfigPath,
                        optional: false,
                        reloadOnChange: true
                    );
                    eatArg = true;
                    continue;
                }

                // -E enables environment variables sourced configuration
                if (args[i] == "-E")
                {
                    seenConfig = true;
                    // add env variables
                    builder.Configuration.AddEnvironmentVariables(prefix: "RCX_");
                    continue;
                }

                Console.WriteLine($"Unknown argument: {args[i]}");
                Environment.Exit(-1);
            }
        }

        return builder;
    }
}
