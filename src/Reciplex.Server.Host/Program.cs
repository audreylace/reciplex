using Microsoft.AspNetCore.HttpOverrides;
using NodaTime;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Database;
using Reciplex.Server.Host;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.Utility;

var builder = WebApplication.CreateBuilder(args);

builder.ParseAndApplyArgs(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

builder.AddShortIds();

builder.Services.ConfigureOptions<ConfigureGlobalJsonHandling>();

builder.ConfigureDataProtection();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IClock>(SystemClock.Instance);

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

#if DEBUG
if (builder.Environment.IsDevelopment())
{
    builder.AddApplicationDbContextForDebug();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.AddAuthenticationDebugOptions();
}
else
{
#endif
    builder.AddSqlite3ApplicationDbContext();

#if DEBUG
}
#endif

builder.Services.Configure<RoutingOptions>(
    builder.Configuration.GetSection(RoutingOptions.SectionPath)
);

builder.AddOpenIdConnect();

var app = builder.Build();

var toRunBeforeStart = app.Services.GetServices<IRunBeforeAppStartup>();
foreach (IRunBeforeAppStartup service in toRunBeforeStart)
{
    await service.RunBeforeStartupAsync(CancellationToken.None);
}

app.Use(
    async (context, next) =>
    {
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append(
            "Permissions-Policy",
            "geolocation=(), camera=(), microphone=()"
        );

        await next();
    }
);

app.UseRedirectOnError();
app.UseAuthentication();

#if DEBUG
if (builder.Environment.IsDevelopment())
{
    app.UseUserDebugMocking();
    app.UseSwagger();
    app.UseSwaggerUI();
}
#endif

app.UseHsts();

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

app.UseAuthorization();
app.MapGroup("/api/v1").MapControllers();
app.MapStaticAssets();
app.MapFallbackToFile("index.html");

app.Run();
