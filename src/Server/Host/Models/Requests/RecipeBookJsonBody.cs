namespace Reciplex.Server.Host.Models.Requests;

public class RecipeBookJsonBody
{
    public required string Name { get; init; }
    public required string ShortDescription { get; init; }
}
