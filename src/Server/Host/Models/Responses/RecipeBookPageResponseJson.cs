namespace Reciplex.Server.Host.Models.Responses;

/// <summary>
/// A page of recipe books
/// </summary>
public class RecipeBookPageResponseJson
{
    /// <summary>
    /// Books in the response
    /// </summary>
    public required IList<RecipeBookJson> RecipeBooks { get; init; }

    /// <summary>
    /// List of users linked to by <see cref="RecipeBooks"/>
    /// </summary>
    public required IList<UserJson> Users { get; init; }

    /// <summary>
    /// If there is a next page, use this ID to get it
    /// </summary>
    public required string? IdForNextPage { get; init; }

    /// <summary>
    /// If there is a previous page, use this ID to get it
    /// </summary>
    public required string? IdForPreviousPage { get; init; }
}
