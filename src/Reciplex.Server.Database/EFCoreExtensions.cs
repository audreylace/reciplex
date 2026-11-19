using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database;

internal static class EFCoreExtensions
{
    /// <summary>
    /// Conditionally enables tracking when <paramref name="enableTrackingFlag"/> is true
    /// </summary>
    /// <typeparam name="T">the entity type</typeparam>
    /// <param name="this">An IQueryable instance to modify</param>
    /// <param name="enableTrackingFlag">flag which when true enables tracking</param>
    /// <returns><paramref name="this"/> with tracking enabled or disabled per <paramref name="enableTrackingFlag"/></returns>
    internal static IQueryable<T> ConditionalTracking<T>(
        this IQueryable<T> @this,
        bool enableTrackingFlag
    )
        where T : class
    {
        if (enableTrackingFlag)
        {
            return @this;
        }

        return @this.AsNoTracking();
    }
}
