using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Reciplex.Server.Host.RecipeKeyUtils;

public class ConfigureRecipeKeyJsonHandling(IStringRecipeKeyInterop recipeKeyInterop)
    : IConfigureOptions<JsonOptions>
{
    public void Configure(JsonOptions options)
    {
        options.SerializerOptions.Converters.Add(new RecipeKeyJsonConverter(recipeKeyInterop));
    }
}
