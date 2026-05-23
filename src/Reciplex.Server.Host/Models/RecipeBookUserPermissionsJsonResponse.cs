using System.Diagnostics.CodeAnalysis;
using Reciplex.Server.Database.RecipeBooksDomain;

namespace Reciplex.Server.Host.Models;

/// <summary>
/// Permission object for additional users who don't own the book
/// but have access.
/// </summary>
public class RecipeBookUserPermissionsJsonResponse
{
    /// <summary>
    /// User key
    /// </summary>
    public required string UserKey { get; init; }

    /// <summary>
    /// Display name of the user
    /// </summary>
    public required string UserDisplayName { get; init; }

    /// <summary>
    /// Books primary key.
    /// </summary>
    public required string BookKey { get; init; }

    /// <summary>
    /// User has access to the book. If false
    /// then all other permissions are ignored.
    /// </summary>
    public required bool MayViewBook { get; init; }

    /// <summary>
    /// User can edit book information
    /// </summary>
    public required bool MayEditBook { get; init; }

    /// <summary>
    /// If the owner has reviewed this entry yet.
    /// </summary>
    public required bool Reviewed { get; init; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public RecipeBookUserPermissionsJsonResponse() { }

    /// <summary>
    /// Constructs JSON response from dao
    /// </summary>
    /// <param name="dao">the dao</param>
    [SetsRequiredMembers]
    public RecipeBookUserPermissionsJsonResponse(RecipeBookUserPermissionsDao dao)
    {
        UserKey = dao.UserKey;
        BookKey = dao.BookKey;
        MayViewBook = dao.MayViewBook;
        MayEditBook = dao.MayEditBook;
        Reviewed = dao.Reviewed;
        UserDisplayName = dao.UserDisplayName;
    }
}
