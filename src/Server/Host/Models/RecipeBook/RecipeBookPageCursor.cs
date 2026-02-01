using System.Text.Json.Serialization;
using Reciplex.Server.Host.Models.PagingUtils;
using Reciplex.Server.RecipeServices.RecipeBooks;

namespace Reciplex.Server.Host.Models.RecipeBook;

public class RecipeBookPageCursor
{
    [JsonPropertyName("going")]
    public required NavigationDirection GoingQueryParam { get; init; }

    [JsonPropertyName("index")]
    public required RecipeBookKey PageIndex { get; init; }
}
