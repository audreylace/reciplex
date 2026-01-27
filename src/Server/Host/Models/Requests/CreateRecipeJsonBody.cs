namespace Reciplex.Server.Host.Models.Requests;

public class CreateRecipeJsonBody
{
    public required string Name { get; init; }
    public required string ShortDescription { get; init; }
    public required string Details { get; init; }
    public required string BookId { get; init; }
}
