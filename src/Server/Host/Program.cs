using Recipe.Database;
using Reciplex.Server.Host.AccessControl;
using Reciplex.Server.Host.RecipeBookKeyUtils;
using Reciplex.Server.Host.RecipeKeyUtils;
using Reciplex.Server.Host.Services.StringIdInterop;
using Reciplex.Server.Host.UserKeyUtils;
using Reciplex.Server.Host.Utils.HttpResults;
using Sqids;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// base abstraction for marshaling ids to and from long values
builder.Services.AddTransient<IStringIdInterop, SquidsStringIdInterop>();
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

// service for marshalling keys to and from strings
builder.Services.AddTransient<IStringRecipeKeyInterop, StringRecipeKeyInterop>();
builder.Services.AddTransient<IStringRecipeBookKeyInterop, StringRecipeBookKeyInterop>();
builder.Services.AddTransient<IStringUserKeyInterop, StringUserKeyInterop>();

// add key marshaling to the json layer
builder.Services.ConfigureOptions<ConfigureRecipeKeyJsonHandling>();
builder.Services.ConfigureOptions<ConfigureRecipeBookKeyJsonHandling>();
builder.Services.ConfigureOptions<ConfigureUserKeyJsonHandling>();

// response factories for controllers
builder.Services.AddTransient<IRecipeProblemFactory, RecipeProblemFactory>();
builder.Services.AddTransient<IRecipeBookProblemFactory, RecipeBookProblemFactory>();

builder.Services.AddControllers(o =>
{
    // custom binders for marshalling keys inside requests
    o.ModelBinderProviders.Add(new RecipeKeysBinder());
    o.ModelBinderProviders.Add(new RecipeBookKeysBinder());
});

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
