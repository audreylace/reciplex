using System.Diagnostics.CodeAnalysis;
using Reciplex.Server.Database.RecipesDomain;

namespace Reciplex.Server.Host.Models;

/// <summary>
/// Information about a recipe
/// </summary>
public class RecipeJsonResponse
{
    /// <summary>
    /// The unique key identifying this recipe
    /// </summary>
    public required string RecipeKey { get; init; }

    /// <summary>
    /// The recipes name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// A short plain text description of the recipe
    /// </summary>
    public required string ShortDescription { get; init; }

    /// <summary>
    /// The recipes details stored as a markdown document
    /// </summary>
    public required string Details { get; init; }

    /// <summary>
    /// The unique key of the book that stores this recipe
    /// </summary>
    public required string BookKey { get; init; }

    /// <summary>
    /// When the recipe was created
    /// </summary>
    public required NodaTime.Instant Created { get; init; }

    /// <summary>
    /// When the recipe was last modified
    /// </summary>
    public required NodaTime.Instant LastModified { get; init; }

    /// <summary>
    /// Tag for optimistic concurrency
    /// </summary>
    public required string ConcurrencyTag { get; init; }

    /// <summary>
    /// True when the end user may edit this recipe. This implies the ability to delete this recipe from the book.
    /// </summary>
    public required bool MayEdit { get; init; }

    /// <summary>
    /// Constructor for creating a recipe json model from a <see cref="RecipeDao"/>
    /// </summary>
    /// <param name="recipe">the source recipe data used to populate this model</param>
    [SetsRequiredMembers]
    public RecipeJsonResponse(RecipeDao recipe)
    {
        RecipeKey = recipe.Id;
        Name = recipe.Name;
        ShortDescription = recipe.ShortDescription;
        Details = recipe.Details;
        BookKey = recipe.BookId;
        Created = recipe.Created;
        LastModified = recipe.LastModified;
        ConcurrencyTag = recipe.ConcurrencyTag;
        MayEdit = recipe.MayEdit;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public RecipeJsonResponse() { }
}
