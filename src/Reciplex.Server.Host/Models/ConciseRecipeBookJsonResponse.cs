namespace Reciplex.Server.Host.Models;

/// <summary>
/// Concise representation of a recipe book
/// </summary>
public class ConciseRecipeBookJsonResponse
{
    /// <summary>
    /// the book id as a string
    /// </summary>
    public required string BookKey { get; init; }

    /// <summary>
    /// the name of the book
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// the book's short description
    /// </summary>
    public required string ShortDescription { get; init; }
}
