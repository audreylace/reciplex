using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Reciplex.Server.Host.RecipeBookKeyUtils;

public class ConfigureRecipeBookKeyJsonHandling(IStringRecipeBookKeyInterop recipeKeyInterop)
    : IConfigureOptions<JsonOptions>
{
    public void Configure(JsonOptions options)
    {
        options.SerializerOptions.Converters.Add(new RecipeBookKeyJsonConverter(recipeKeyInterop));
    }
}
