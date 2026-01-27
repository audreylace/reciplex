using System.Diagnostics.CodeAnalysis;
using Reciplex.Server.RecipeServices.RecipeBooks.Models;

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

    /// <summary>
    /// Default constructor
    /// </summary>
    public RecipeBookJson() { }

    /// <summary>
    /// Builds an instance of this object from <paramref name="book"/>
    /// </summary>
    /// <param name="book">the dao data to transfer</param>
    [SetsRequiredMembers]
    public RecipeBookJson(RecipeBookDao book)
    {
        BookId = book.Id;
        Name = book.Name;
        ShortDescription = book.ShortDescription;
        OwnerUserId = book.OwnerUserId;
        Created = book.CreateTime.ToString("o");
        LastModified = book.LastUpdated.ToString("o");
        ConcurrencyTag = book.ConcurrencyTag;
        Permissions = new()
        {
            EditInformation = book.AccessPermissions.HasFlag(
                RecipeBookAccessPermissions.EditBookInformation
            ),
            AddRecipe = book.AccessPermissions.HasFlag(RecipeBookAccessPermissions.AddRecipes),
            Delete = book.AccessPermissions.HasFlag(RecipeBookAccessPermissions.DeleteBook),
        };
    }
}
