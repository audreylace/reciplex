using Reciplex.Server.Database.RecipeBooksDomain;

namespace Reciplex.Server.Host.Models;

/// <summary>
/// Result of endpoints for individual user managed access to a recipe book
/// </summary>
public class RequestAccessToRecipeBookStatusJsonResponse
{
    /// <summary>
    /// The recipe book key
    /// </summary>
    public required string BookKey { get; init; }

    /// <summary>
    /// Name of the book
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Book's short description
    /// </summary>
    public required string ShortDescription { get; init; }

    /// <summary>
    /// The share status
    /// </summary>
    public required RequestAccessToRecipeBookStatus Status { get; init; }
}
