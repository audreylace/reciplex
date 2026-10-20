namespace Reciplex.Server.Database.SearchIndexTrackingDomain;

/// <summary>
/// Bridge object for an invalid row
/// </summary>
/// <param name="RowId">The source row id</param>
public sealed record class InvalidRowExtractedFromDatabase(long RowId) : IExtractedSearchRow;
