using System.Diagnostics.CodeAnalysis;
using Reciplex.Server.RecipeServices.RecipeBooks;
using Reciplex.Server.RecipeServices.Recipes;
using Reciplex.Server.RecipeServices.Recipes.Models;

namespace Reciplex.Server.Host.Models.Recipe;

public class RecipeJson
{
    public required RecipeKey RecipeId { get; init; }
    public required string Name { get; init; }
    public required string ShortDescription { get; init; }
    public required string Details { get; init; }
    public required RecipeBookKey BookId { get; init; }
    public required NodaTime.Instant Created { get; init; }
    public required NodaTime.Instant LastModified { get; init; }
    public required string ConcurrencyTag { get; init; }
    public required bool MayEdit { get; init; }

    [SetsRequiredMembers]
    public RecipeJson(RecipeDao recipe)
    {
        RecipeId = recipe.Id;
        Name = recipe.Name;
        ShortDescription = recipe.ShortDescription;
        Details = recipe.Details;
        BookId = recipe.BookId;
        Created = recipe.Created;
        LastModified = recipe.LastModified;
        ConcurrencyTag = recipe.ConcurrencyTag;
        MayEdit = recipe.MayEdit;
    }

    public RecipeJson() { }
}
