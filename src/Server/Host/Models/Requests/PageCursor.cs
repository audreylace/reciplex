using Microsoft.AspNetCore.Mvc;
using Reciplex.Server.Host.Validation;

namespace Reciplex.Server.Host.Models.Requests;

/// <summary>
/// Page cursor
/// </summary>
public class PageCursor
{
    [FromQuery(Name = "going")]
    public NavigationDirection? Going { get; init; }

    public NavigationDirection.DirectionValue GoingDirection =>
        Going?.Direction ?? NavigationDirection.DirectionValue.Forwards;

    [NotEmptyNorWhitespace]
    [FromQuery(Name = "index")]
    public string? Index { get; init; }

    public string? ComputeIdForNextPage<TEntryType>(
        Func<TEntryType, string> keySelector,
        IList<TEntryType> pageData
    )
    {
        return pageData.Count > 0 ? keySelector(pageData[^1])
            : Going?.Direction == NavigationDirection.DirectionValue.Backwards ? Index
            : null;
    }

    public string? ComputeIdForPreviousPage<TEntryType>(
        Func<TEntryType, string> keySelector,
        IList<TEntryType> pageData
    )
    {
        return pageData.Count > 0 ? keySelector(pageData[0])
            : Going?.Direction == NavigationDirection.DirectionValue.Forwards ? Index
            : null;
    }

    public bool ShouldRunNextPageQuery(string? id)
    {
        // if we are moving forwards (next page) then the initial query,
        // if empty, means there is not any more possible results
        if (GoingDirection == NavigationDirection.DirectionValue.Forwards && id is null)
        {
            return false;
        }

        return true;
    }

    public bool ShouldRunPreviousPageQuery(string? id)
    {
        // if we are moving backwards (previous page) then the initial query,
        // if empty, means there is not any more possible results in going back
        if (GoingDirection == NavigationDirection.DirectionValue.Backwards && id is null)
        {
            return false;
        }

        return true;
    }
}
