namespace Reciplex.Server.Host.Models.Recipe;

public class CreateOrUpdateRecipeJsonBody
{
    public required string Name { get; init; }
    public required string ShortDescription { get; init; }
    public required string Details { get; init; }
}
