using System.Text.Json;
using System.Text.Json.Serialization;
using Reciplex.Server.RecipeServices.RecipeBooks;

namespace Reciplex.Server.Host.RecipeBookKeyUtils;

public class RecipeBookKeyJsonConverter(IStringRecipeBookKeyInterop recipeBookKeyInterop)
    : JsonConverter<RecipeBookKey>
{
    public override RecipeBookKey Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        string s = reader.GetString() ?? throw new JsonException();
        return recipeBookKeyInterop.AsKey(s) ?? throw new JsonException();
    }

    public override void Write(
        Utf8JsonWriter writer,
        RecipeBookKey value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStringValue(recipeBookKeyInterop.AsString(value));
    }
}
