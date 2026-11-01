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
}
