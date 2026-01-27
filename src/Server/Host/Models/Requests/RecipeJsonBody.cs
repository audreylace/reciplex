using Microsoft.AspNetCore.Http.HttpResults;

namespace Reciplex.Server.Host.Models.Requests;

public class RecipeJsonBody
{
    public required string Name { get; init; }
    public required string ShortDescription { get; init; }
    public required string Details { get; init; }
}
