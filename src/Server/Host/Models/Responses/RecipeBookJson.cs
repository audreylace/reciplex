namespace Reciplex.Server.Host.Models.Responses;

public class RecipeBookJson
{
    public required string BookId { get; init; }
    public required string Name { get; init; }
    public required string ShortDescription { get; init; }
    public required string OwnerUserId { get; init; }
    public required RecipeBookPermissionsJson Permissions { get; init; }
    public required string Created { get; init; }
    public required string LastModified { get; init; }
    public required string ConcurrencyTag { get; init; }
}
