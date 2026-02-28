using System.Text.Json;
using System.Text.Json.Serialization;
using Reciplex.Server.RecipeServices.Recipes;

namespace Reciplex.Server.Host.Utils.RecipeKeyUtils;

public class RecipeKeyJsonConverter(IStringRecipeKeyInterop recipeKeyInterop)
    : JsonConverter<RecipeKey>
{
    public override RecipeKey Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        string s = reader.GetString() ?? throw new JsonException();
        return recipeKeyInterop.AsKey(s) ?? throw new JsonException();
    }

    public override void Write(
        Utf8JsonWriter writer,
        RecipeKey value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStringValue(recipeKeyInterop.AsString(value));
    }
}
