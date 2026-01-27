using System.Diagnostics.CodeAnalysis;
using Reciplex.Server.RecipeServices.Recipes.Models;

namespace Reciplex.Server.Host.Models.Responses;

public class RecipeJson
{
    public required string RecipeId { get; init; }
    public required string Name { get; init; }
    public required string ShortDescription { get; init; }
    public required string Details { get; init; }
    public required string BookId { get; init; }
    public required string Created { get; init; }
    public required string LastModified { get; init; }
    public required string ConcurrencyTag { get; init; }
    public required RecipePermissionsJson Permissions { get; init; }

    [SetsRequiredMembers]
    public RecipeJson(RecipeDao recipe)
    {
        RecipeId = recipe.Id;
        Name = recipe.Name;
        ShortDescription = recipe.ShortDescription;
        Details = recipe.Details;
        BookId = recipe.BookId;
        Created = recipe.CreateTime.ToString("o");
        LastModified = recipe.LastUpdated.ToString("o");
        ConcurrencyTag = recipe.ConcurrencyTag;
        Permissions = new()
        {
            Delete = recipe.AccessPermissions.HasFlag(RecipeAccessPermissions.DeleteRecipe),
            Edit = recipe.AccessPermissions.HasFlag(RecipeAccessPermissions.EditRecipe),
        };
    }

    public RecipeJson() { }
}
