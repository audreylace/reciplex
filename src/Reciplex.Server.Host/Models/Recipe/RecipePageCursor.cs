using System.Text.Json.Serialization;
using Reciplex.Server.Host.Models.PagingUtils;
using Reciplex.Server.RecipeServices.RecipeBooks;
using Reciplex.Server.RecipeServices.Recipes;

namespace Reciplex.Server.Host.Models.Recipe;

public class RecipePageCursor
{
    [JsonPropertyName("going")]
    public required NavigationDirection GoingQueryParam { get; init; }

    [JsonPropertyName("index")]
    public required RecipeKey PageIndex { get; init; }

    [JsonPropertyName("book")]
    public required RecipeBookKey BookId { get; init; }
}
