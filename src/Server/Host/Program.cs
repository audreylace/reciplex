using System.Security.Claims;
using Recipe.Database;
using Reciplex.Server.Host.AccessControl;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAuthentication();

if (builder.Environment.IsDevelopment())
{
    builder.AddApplicationDbContextForDebug();
}
else
{
    builder.AddApplicationDbContext();
}

var app = builder.Build();

app.UseAuthentication();
if (builder.Environment.IsDevelopment())
{
    app.UseUserDebugMocking();
}
else
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapGroup("/api/v1").MapControllers();

app.Run();
