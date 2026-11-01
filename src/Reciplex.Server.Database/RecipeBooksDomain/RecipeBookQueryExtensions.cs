using Microsoft.EntityFrameworkCore;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Extensions on for querying <see cref="RecipeBookDbObject"/>
/// </summary>
static class RecipeBookQueryExtensions
{
    /// <summary>
    /// Limits query to books that are not deleted.
    /// </summary>
    /// <param name="query">the query instance to modify</param>
    /// <returns>the query the filter applied</returns>
    public static IQueryable<RecipeBookDbObject> NotDeleted(
        this IQueryable<RecipeBookDbObject> query
    )
    {
        return query.Where(book => !book.IsDeleted);
    }

    /// <summary>
    /// Limits query to books that are deleted.
    /// </summary>
    /// <returns>the query the filter applied</returns>
    public static IQueryable<RecipeBookDbObject> IsDeleted(
        this IQueryable<RecipeBookDbObject> query
    )
    {
        return query.Where(book => book.IsDeleted);
    }

    /// <summary>
    /// Limits query to books that are deleted.
    /// </summary>
    /// <returns>the query with the filter applied</returns>
    public static IQueryable<RecipeBookDbObject> WithConcurrencyTag(
        this IQueryable<RecipeBookDbObject> query,
        string concurrencyTag
    )
    {
        return query.Where(book => book.ConcurrencyTag == concurrencyTag);
    }

    /// <summary>
    /// Scopes the set of books to those that the user owns
    /// or <see cref="BookPermissionFlags.MayViewBook"/> is true
    /// </summary>
    /// <param name="userId">the id of the user to scope access to</param>
    /// <returns>the query with the filter applied</returns>
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
    /// <param name="query">the query to modify</param>
    /// <param name="bookId">the book primary key</param>
    /// <returns>the query with the filter applied</returns>
    public static IQueryable<RecipeBookDbObject> WithBookId(
        this IQueryable<RecipeBookDbObject> query,
        long bookId
    )
    {
        return query.Where(u => u.Id == bookId);
    }

    /// <summary>
    /// Converts the db model into the application model
    /// </summary>
    /// <param name="book">the book db object</param>
    /// <param name="userId">the user id</param>
    /// <param name="shareEntry">the share entry db object</param>
    /// <returns>the application model created from the db object</returns>
    internal static BookWithMaterializedPermissions MaterializeBookAndPermissions(
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

    /// <summary>
    /// Gets books with associated user permission entries
    /// </summary>
    /// <param name="dbContext">the db connection</param>
    /// <param name="bookId">book id</param>
    /// <param name="userId">user id</param>
    /// <param name="ct">async cancellation token</param>
    /// <param name="withTracking">set to true if tracking should be enabled. Defaults to false.</param>
    /// <returns>the result data</returns>
    internal static async Task<BookWithMaterializedPermissions?> GetBookWithUserPermissionsAsync(
        this ApplicationDbContext dbContext,
        long bookId,
        long userId,
        CancellationToken ct,
        bool withTracking = false
    )
    {
        var accesses = dbContext.RecipeBookAccessEntries.Where(a =>
            a.UserFk == userId && a.RecipeBookFk == bookId
        );
        var books = dbContext.RecipeBooks.Where(b => b.Id == bookId && !b.IsDeleted);

        var bookQuery =
            from book in books
            join access in accesses on book.Id equals access.RecipeBookFk into accessGroup
            from access in accessGroup.DefaultIfEmpty()
            select new { Book = book, Access = access };

        var bookData = await bookQuery.ConditionalTracking(withTracking).FirstOrDefaultAsync(ct);

        return bookData is not null
            ? MaterializeBookAndPermissions(bookData.Book, userId, bookData.Access)
            : null;
    }
}
