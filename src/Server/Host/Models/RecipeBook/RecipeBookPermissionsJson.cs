namespace Reciplex.Server.Host.Models.RecipeBook;

public class RecipeBookPermissionsJson
{
    public required bool Delete { get; init; }
    public required bool AddRecipe { get; init; }
    public required bool EditInformation { get; init; }
}
