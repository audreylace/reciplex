using System.Diagnostics.CodeAnalysis;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Recipe book access request status
/// </summary>
public class RecipeBookAccessRequestStatus()
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bookKey">the book string key</param>
    /// <param name="book">the book object</param>
    /// <param name="userAccessEntry">the access entry if this book is not owned by the current user</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="userAccessEntry"/> does not grant access to the book</exception>
    [SetsRequiredMembers]
    internal RecipeBookAccessRequestStatus(
        string bookKey,
        RecipeBookDbObject book,
        AdditionalBookUserAccessDbObject? userAccessEntry
    )
        : this()
    {
        BookKey = bookKey;
        Name = book.Name;
        ShortDescription = book.ShortDescription;
        Status = userAccessEntry switch
        {
            { MayViewBook: false, Reviewed: true } => throw new ArgumentException(
                "user does not have access to book",
                nameof(userAccessEntry)
            ),
            { Reviewed: false } => RequestAccessToRecipeBookStatus.Pending,
            { Reviewed: true } => RequestAccessToRecipeBookStatus.Approved,
            _ => RequestAccessToRecipeBookStatus.NoRequestInProgress,
        };
    }

    /// <summary>
    /// Name of the book
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Book description
    /// </summary>
    public required string ShortDescription { get; init; }

    /// <summary>
    /// The book key
    /// </summary>
    public required string BookKey { get; init; }

    /// <summary>
    /// Status of the request
    /// </summary>
    public required RequestAccessToRecipeBookStatus Status { get; init; }
}
