using Microsoft.AspNetCore.HttpOverrides;
using NodaTime;
using Recipe.Database;
using Reciplex.Server.Host;
using Reciplex.Server.Host.AccessControl;

var builder = WebApplication.CreateBuilder(args);

bool eatArg = false;
bool seenConfig = false;

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

// Add services to the container.

builder.AddShortIds();

builder.Services.ConfigureOptions<ConfigureGlobalJsonHandling>();

builder.ConfigureDataProtection();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

builder.Services.AddControllers();

builder.Services.AddSingleton<IClock>(SystemClock.Instance);

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

app.UseAuthentication();

#if DEBUG
if (builder.Environment.IsDevelopment())
{
    app.UseUserDebugMocking();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
#endif

    RoutingOptions routingOptions = new();
    app.Configuration.Bind(RoutingOptions.SectionPath, routingOptions);

    if (routingOptions.TrustProxy)
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

#if DEBUG
}
#endif

app.UseAuthorization();
app.MapGroup("/api/v1").MapControllers();
app.MapStaticAssets();
app.MapFallbackToFile("index.html");

app.Run();
