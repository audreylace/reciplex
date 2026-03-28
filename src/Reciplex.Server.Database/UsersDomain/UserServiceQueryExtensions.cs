using System.Data;
using FluentValidation;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.UsersDomain;

internal static class UserServiceQueryExtensions
{
    public static IQueryable<UserDbObject> UserNotDeleted(this IQueryable<UserDbObject> query)
    {
        return query.Where(u => u.Deleted == null);
    }

    public static IQueryable<RecipeBookDbObject> OwningUserNotDeleted(
        this IQueryable<RecipeBookDbObject> query
    )
    {
        return query.Where(b => b.Owner != null && b.Owner.Deleted == null);
    }
}
