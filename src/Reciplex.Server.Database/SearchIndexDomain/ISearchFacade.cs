using Reciplex.Server.Database.Results;

namespace Reciplex.Server.SearchExporter;

/// <summary>
/// Facade providing search services. Requires the search index feature to be active.
/// </summary>
public interface ISearchIndexFacade
{
    /// <summary>
    /// Runs a search against the index
    /// </summary>
    /// <param name="userId">the if of the searching user</param>
    /// <param name="args">the search arguments</param>
    /// <param name="ct">cancels the async operation</param>
    /// <returns>the search result</returns>
    Task<
        DatabaseResultVariant<
            BookNotFoundResult,
            ValidationFailureResult,
            FeatureNotEnabledResult,
            SuccessResult<SearchResultDao>
        >
    > SearchAsync(long userId, SearchIndexAsyncArgs args, CancellationToken ct);
}
