using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Extensions on for querying <see cref="RecipeBookDbObject"/>
/// </summary>
static class RecipeBookQueryExtensions
{
    /// <summary>
    /// Limits query to books that are not deleted. Filters out books whose owner is deleted.
    /// </summary>
    public static IQueryable<RecipeBookDbObject> NotDeleted(
        this IQueryable<RecipeBookDbObject> query
    )
    {
        return query.Where(book => book.Deleted == null && book.Owner!.Deleted == null);
    }

    /// <summary>
    /// Scopes the set of books to those that the user owns
    /// or <see cref="BookPermissionFlags.MayViewBook"/> is true
    /// </summary>
    /// <param name="userId">the id of the user to scope access to</param>
    public static IQueryable<RecipeBookDbObject> UserHasAccess(
        this IQueryable<RecipeBookDbObject> query,
        long userId
    )
    {
        return query.Where(book =>
            book.OwnerFk == userId
            || book.AdditionalUsers.Any(access =>
                access.MayViewBook && access.UserFk == userId && access.Reviewed
            )
        );
    }

    /// <summary>
    /// Filters query down to book with primary key <paramref name="bookId"/>
    /// </summary>
    /// <param name="bookId">the book primary key</param>
    public static IQueryable<RecipeBookDbObject> WithBookId(
        this IQueryable<RecipeBookDbObject> query,
        long bookId
    )
    {
        return query.Where(u => u.Id == bookId);
    }

    /// <summary>
    /// Returns the first book from the query and computes its permissions for user <paramref name="userId"/>
    /// </summary>
    /// <param name="userId">the user ID to compute permissions for</param>
    /// <param name="ct">token to cancel the operation</param>
    public static async Task<BookWithMaterializedPermissions?> MaterializeWithPermissionsFirstOrDefaultAsync(
        this IQueryable<RecipeBookDbObject> query,
        long userId,
        CancellationToken ct = default
    )
    {
        var queryResult = await query
            .Select(b => new
            {
                Book = b,
                AdditionalBookUserAccessDbObject = b.OwnerFk == userId
                    ? null
                    : b.AdditionalUsers.FirstOrDefault(u => u.UserFk == userId),
            })
            .FirstOrDefaultAsync(ct);

        if (queryResult is null)
        {
            return null;
        }

        return MaterializeBookAndPermissions(
            queryResult.Book,
            userId,
            queryResult.AdditionalBookUserAccessDbObject
        );
    }

    /// <summary>
    /// Returns a series of books with computed permissions information
    /// </summary>
    /// <param name="userId">the user ID to compute permissions for</param>
    /// <param name="ct">token to cancel the operation</param>
    public static async IAsyncEnumerable<BookWithMaterializedPermissions> MaterializeWithPermissionsAsyncEnumerable(
        this IQueryable<RecipeBookDbObject> query,
        long userId,
        [EnumeratorCancellation] CancellationToken ct = default
    )
    {
        await foreach (
            var queryResult in query
                .Select(b => new
                {
                    Book = b,
                    AdditionalBookUserAccessDbObject = b.OwnerFk == userId
                        ? null
                        : b.AdditionalUsers.FirstOrDefault(u => u.UserFk == userId),
                })
                .AsAsyncEnumerable()
                .WithCancellation(ct)
        )
        {
            yield return MaterializeBookAndPermissions(
                queryResult.Book,
                userId,
                queryResult.AdditionalBookUserAccessDbObject
            );
        }
    }

    /// <summary>
    /// Gets a book only if it is not deleted. Determines user's access if any.
    /// </summary>
    /// <param name="bookId">the book id</param>
    /// <param name="userId">the user id</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>Book is returned if it is not deleted. Calling code still needs to do manual access checks.</returns>
    public static Task<BookWithMaterializedPermissions?> GetBookAndPermissionsAsync(
        this IQueryable<RecipeBookDbObject> query,
        long bookId,
        long userId,
        CancellationToken ct
    )
    {
        return query
            .NotDeleted()
            .WithBookId(bookId)
            .MaterializeWithPermissionsFirstOrDefaultAsync(userId, ct);
    }

    private static BookWithMaterializedPermissions MaterializeBookAndPermissions(
        RecipeBookDbObject book,
        long userId,
        AdditionalBookUserAccessDbObject? shareEntry
    )
    {
        if (book.OwnerFk == userId)
        {
            return new(book, BookPermissionFlags.OwnerPermissions(), null);
        }

        if (shareEntry is null || !shareEntry.Reviewed || !shareEntry.MayViewBook)
        {
            return new(book, BookPermissionFlags.NoAccess(), shareEntry);
        }

        return new(book, BookPermissionFlags.SharedPermissions(shareEntry.MayEditBook), shareEntry);
    }
}
