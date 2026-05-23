using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Extensions on <see cref="AdditionalBookUserAccessDbObject"/>
/// </summary>
static class AdditionalBookUserAccessDbObjectQueryExtensions
{
    /// <summary>
    /// Filters out <see cref="AdditionalBookUserAccessDbObject"/>
    /// where either the book or user object is deleted
    /// </summary>
    public static IQueryable<AdditionalBookUserAccessDbObject> NotDeleted(
        this IQueryable<AdditionalBookUserAccessDbObject> query
    )
    {
        return query.Where(e => e.User!.Deleted == null && e.RecipeBook!.Deleted == null);
    }

    /// <summary>
    /// Scopes the list of <see cref="AdditionalBookUserAccessDbObject"/>
    /// to only those related to <see cref="RecipeBookDbObject"/> with
    /// primary key <paramref name="bookId"/>.
    /// </summary>
    /// <param name="bookId">the book primary key</param>
    public static IQueryable<AdditionalBookUserAccessDbObject> ScopeToBook(
        this IQueryable<AdditionalBookUserAccessDbObject> query,
        long bookId
    )
    {
        return query.Where(e => e.RecipeBookFk == bookId);
    }

    /// <summary>
    /// Scopes the list of <see cref="AdditionalBookUserAccessDbObject"/>
    /// to only those related to <see cref="UserDbObject"/> with
    /// primary key <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">the book primary key</param>
    public static IQueryable<AdditionalBookUserAccessDbObject> WithUserId(
        this IQueryable<AdditionalBookUserAccessDbObject> query,
        long userId
    )
    {
        return query.Where(e => e.UserFk == userId);
    }
}
