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
    /// <param name="query">the query to chain the condition to</param>
    /// <returns><paramref name="query"/> with the condition chained</returns>
    public static IQueryable<UserDbObject> UserNotDeleted(this IQueryable<UserDbObject> query)
    {
        return query.Where(u => !u.IsDeleted);
    }

    /// <summary>
    /// Limits result to the <see cref="UserDbObject"/> with primary key <paramref name="userId"/>
    /// </summary>
    /// <param name="userId">the user id primary key</param>
    public static IQueryable<UserDbObject> WithId(this IQueryable<UserDbObject> query, long userId)
    {
        return query.Where(u => u.Id == userId);
    }
}
