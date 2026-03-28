using NodaTime;
using Recipe.Database;
using Reciplex.Server.Abstractions.StringIdProvider;
using Reciplex.Server.Host;
using Reciplex.Server.Host.AccessControl;
using Sqids;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// base abstraction for marshaling ids to and from long values
builder.Services.AddSingleton<IStringIdProvider, SquidsStringIdProvider>();
builder.Services.AddSingleton(
    new SqidsEncoder<long>(
        new()
        {
            // todo - these should be app settings
            Alphabet = "yPX4xMlq8kwfOde1J6gEKrj2AsCi7GUNpoHzWcV39FbTaBm5unYv0RStDQZLIh",
            MinLength = 8,
        }
    )
);

builder.Services.ConfigureOptions<ConfigureGlobalJsonHandling>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

builder.Services.AddControllers();

builder.Services.AddSingleton<IClock>(SystemClock.Instance);

if (builder.Environment.IsDevelopment())
{
    builder.AddApplicationDbContextForDebug();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}
else
{
    builder.AddApplicationDbContext();
}

builder.AddOpenIdConnect();

var app = builder.Build();

app.UseAuthentication();
if (builder.Environment.IsDevelopment())
{
    app.UseUserDebugMocking();
    app.UseSwagger();
    app.UseSwaggerUI();
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
