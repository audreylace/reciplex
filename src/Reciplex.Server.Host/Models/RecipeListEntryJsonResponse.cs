using System.Diagnostics.CodeAnalysis;
using Reciplex.Server.Database.RecipesDomain;

namespace Reciplex.Server.Host.Models;

/// <summary>
/// Information about a recipe in a list
/// </summary>
public class RecipeListEntryJsonResponse
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
    /// The unique key of the book that stores this recipe
    /// </summary>
    public required string BookKey { get; init; }

    /// <summary>
    /// Constructor for creating a recipe json model from a <see cref="RecipeDao"/>
    /// </summary>
    /// <param name="recipe">the source recipe data used to populate this model</param>
    [SetsRequiredMembers]
    public RecipeListEntryJsonResponse(RecipeListEntryDao recipe)
    {
        RecipeKey = recipe.Id;
        Name = recipe.Name;
        ShortDescription = recipe.ShortDescription;
        BookKey = recipe.BookId;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public RecipeListEntryJsonResponse() { }
}
