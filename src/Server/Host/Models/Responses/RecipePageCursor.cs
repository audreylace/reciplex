using System.Text.Json.Serialization;

namespace Reciplex.Server.Host.Models.Responses;

public class RecipePageCursor
{
    [JsonPropertyName("going")]
    public required string GoingQueryParam { get; init; }

    [JsonPropertyName("index")]
    public required string PageIndex { get; init; }

    [JsonPropertyName("book-id")]
    public required string? BookId { get; init; }
}
