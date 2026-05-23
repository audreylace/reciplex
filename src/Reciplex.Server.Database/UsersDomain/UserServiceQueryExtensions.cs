using System.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Extensions on for querying <see cref="UserDbObject"/>
/// </summary>
internal static class UserServiceQueryExtensions
{
    /// <summary>
    /// Limits results to only undeleted <see cref="UserDbObject"/> objects
    /// </summary>
    public static IQueryable<UserDbObject> UserNotDeleted(this IQueryable<UserDbObject> query)
    {
        return query.Where(u => u.Deleted == null);
    }

    /// <summary>
    /// Limits result to the <see cref="UserDbObject"/> with primary key <paramref name="userId"/>
    /// </summary>
    /// <param name="userId">the user id primary key</param>
    public static IQueryable<UserDbObject> WithId(this IQueryable<UserDbObject> query, long userId)
    {
        return query.Where(u => u.Id == userId);
    }

    /// <summary>
    /// Checks if a user with primary key <paramref name="userId"/> exists and is not deleted.
    /// </summary>
    /// <param name="userId">the user id to check</param>
    /// <param name="ct">cancels the async operation</param>
    /// <returns>true if user exists and is not deleted</returns>
    public static Task<bool> UserExistsNotDeletedAsync(
        this IQueryable<UserDbObject> query,
        long userId,
        CancellationToken ct
    )
    {
        return query.UserNotDeleted().WithId(userId).AnyAsync(ct);
    }
}
