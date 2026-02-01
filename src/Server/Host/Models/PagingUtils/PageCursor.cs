using Microsoft.AspNetCore.Mvc;

namespace Reciplex.Server.Host.Models.PagingUtils;

/// <summary>
/// Page cursor
/// </summary>
public class PageCursor<TKeyType>
{
    /// <summary>
    /// The raw navigation direction decoded from the query
    /// </summary>
    [FromQuery(Name = "going")]
    public NavigationDirection? GoingQueryValue { get; init; }

    /// <summary>
    /// Computed value from <see cref="GoingQueryValue"/>
    /// </summary>
    public NavigationDirection.DirectionValue GoingDirection =>
        GoingQueryValue?.Direction ?? NavigationDirection.DirectionValue.Forwards;

    /// <summary>
    /// The cursor index
    /// </summary>
    [FromQuery(Name = "index")]
    public TKeyType? Index { get; init; }

    /// <summary>
    /// Helper for computing the id that should be used in the query
    /// for determining if there are is data after the current page.
    /// </summary>
    /// <typeparam name="TEntryType">The type to project the ID from</typeparam>
    /// <param name="idProjector">Delegate to project ID from <typeparamref name="TEntryType"/></param>
    /// <param name="pageData">Loaded page data</param>
    /// <returns>
    /// The index that should be used to query for the next page, possibly null.
    /// Check <see cref="ShouldRunNextPageQuery"/>
    /// to determine if the query for the next page should be executed.
    /// </returns>
    public TKeyType? ComputeIdForNextPage<TEntryType>(
        Func<TEntryType, TKeyType> idProjector,
        IList<TEntryType> pageData
    )
    {
        return pageData.Count > 0 ? idProjector(pageData[^1])
            : GoingQueryValue?.Direction == NavigationDirection.DirectionValue.Backwards ? Index
            : default(TKeyType);
    }

    /// <summary>
    /// Helper for computing the id that should be used in the query
    /// for determining if there are is data before the current page.
    /// </summary>
    /// <typeparam name="TEntryType">The type to project the ID from</typeparam>
    /// <param name="idProjector">Delegate to project ID from <typeparamref name="TEntryType"/></param>
    /// <param name="pageData">Loaded page data</param>
    /// <returns>
    /// The index that should be used to query for the previous page, possibly null.
    /// Check <see cref="ShouldRunPreviousPageQuery"/>
    /// to determine if the query for the previous page should be executed.
    /// </returns>
    public TKeyType? ComputeIdForPreviousPage<TEntryType>(
        Func<TEntryType, TKeyType> idProjector,
        IList<TEntryType> pageData
    )
    {
        return pageData.Count > 0 ? idProjector(pageData[0])
            : GoingQueryValue?.Direction == NavigationDirection.DirectionValue.Forwards ? Index
            : default(TKeyType);
    }

    /// <summary>
    /// Helper to determine if the application should query for the next page
    /// </summary>
    /// <param name="id">ID value computed from <see cref="ComputeIdForNextPage"/> </param>
    /// <returns>true if there is could be a next page</returns>
    public bool ShouldRunNextPageQuery(TKeyType? id)
    {
        // if we are moving forwards (next page) then the initial query,
        // if empty, means there is not any more possible results
        if (GoingDirection == NavigationDirection.DirectionValue.Forwards && id is null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Helper to determine if the application should query for the previous page
    /// </summary>
    /// <param name="id">ID value computed from <see cref="ComputeIdForPreviousPage"/> </param>
    /// <returns>true if there is could be a previous page</returns>
    public bool ShouldRunPreviousPageQuery(TKeyType? id)
    {
        // if we are moving backwards (previous page) then the initial query,
        // if empty, means there is not any more possible results in going back
        if (GoingDirection == NavigationDirection.DirectionValue.Backwards && id is null)
        {
            return false;
        }

        return true;
    }

    public async Task<(bool hasNextPage, TKeyType? nextPageCursor)> QueryForNextPage<TEntryType>(
        Func<TEntryType, TKeyType> idProjector,
        IList<TEntryType> pageData,
        Func<TKeyType?, CancellationToken, Task<bool>> query,
        CancellationToken cancellationToken
    )
    {
        bool hasNextPage = false;
        TKeyType? idForNextPage = ComputeIdForNextPage(idProjector, pageData);
        if (ShouldRunNextPageQuery(idForNextPage))
        {
            hasNextPage = await query(idForNextPage, cancellationToken);
        }

        return (hasNextPage, idForNextPage);
    }

    public async Task<(
        bool hasPreviousPage,
        TKeyType? nextPageCursor
    )> QueryForPreviousPage<TEntryType>(
        Func<TEntryType, TKeyType> idProjector,
        IList<TEntryType> pageData,
        Func<TKeyType?, CancellationToken, Task<bool>> query,
        CancellationToken cancellationToken
    )
    {
        bool hasPreviousPage = false;
        TKeyType? idForPreviousPage = ComputeIdForPreviousPage(idProjector, pageData);
        if (ShouldRunPreviousPageQuery(idForPreviousPage))
        {
            hasPreviousPage = await query(idForPreviousPage, cancellationToken);
        }

        return (hasPreviousPage, idForPreviousPage);
    }
}
