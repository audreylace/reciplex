namespace Reciplex.Server.Database.SearchIndexTrackingDomain;

/// <summary>
/// Types returned from extracted search row API
/// </summary>
public interface IExtractedSearchRow
{
    /// <summary>
    /// The source row id
    /// </summary>
    long RowId { get; }
}
