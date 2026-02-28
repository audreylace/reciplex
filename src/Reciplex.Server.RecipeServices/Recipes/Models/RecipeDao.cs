using NodaTime;
using Reciplex.Server.RecipeServices.RecipeBooks;

namespace Reciplex.Server.RecipeServices.Recipes.Models;

/// <summary>
/// Information about a recipe
/// </summary>
public class RecipeDao
{
    /// <summary>
    /// Recipe primary key.
    /// </summary>
    public required RecipeKey Id { get; init; }

    /// <summary>
    /// Name of the recipe
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Recipe short description
    /// </summary>
    public required string ShortDescription { get; init; }

    /// <summary>
    /// The concurrency tag. MUST be transportable via ETAG header.
    /// </summary>
    public required string ConcurrencyTag { get; init; }

    /// <summary>
    /// Last time in UTC the recipe book details were updated
    /// </summary>
    public required Instant LastModified { get; init; }

    /// <summary>
    /// The time in UTC the recipe book was created
    /// </summary>
    public required Instant Created { get; init; }

    /// <summary>
    /// User can edit and delete recipe
    /// </summary>
    public required bool MayEdit { get; set; }

    /// <summary>
    /// The book this recipe is contained in
    /// </summary>
    public required RecipeBookKey BookId { get; init; }

    /// <summary>
    /// The long, freeform, markdown recipe data
    /// </summary>
    public required string Details { get; set; }
}
