namespace Reciplex.Server.Host.Models;

public class RecipeJsonRequest
{
    public required string Name { get; init; }
    public required string ShortDescription { get; init; }
    public required string Details { get; init; }
}
