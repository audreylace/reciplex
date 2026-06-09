using NodaTime;
using Recipe.Database;
using Reciplex.Server.Host;
using Reciplex.Server.Host.AccessControl;

var builder = WebApplication.CreateBuilder(args);

// Add custom config paths from command line
for (int i = 0; i < args.Length; i++)
{
    // Optional config, little c
    if (args[i] == "-c" && i + 1 < args.Length)
    {
        string additionalConfigPath = args[i + 1];

        builder.Configuration.AddJsonFile(
            additionalConfigPath,
            optional: true,
            reloadOnChange: true
        );
    }

    // Mandatory config, big C
    if (args[i] == "-C" && i + 1 < args.Length)
    {
        string additionalConfigPath = args[i + 1];

        builder.Configuration.AddJsonFile(
            additionalConfigPath,
            optional: false,
            reloadOnChange: true
        );
    }
}

// add env variables
builder.Configuration.AddEnvironmentVariables(prefix: "RCX_");

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

    app.UseHttpsRedirection();

#if DEBUG
}
#endif

app.UseAuthorization();
app.MapGroup("/api/v1").MapControllers();
app.MapStaticAssets();
app.MapFallbackToFile("index.html");

app.Run();
