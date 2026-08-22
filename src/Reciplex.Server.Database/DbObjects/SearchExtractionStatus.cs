namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Search extraction status
/// </summary>
public enum SearchExtractionStatus
{
    /// <summary>
    /// Never extracted
    /// </summary>
    NotExtracted = 0,

    /// <summary>
    /// Extraction in progress
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Extraction completed. Check the tag
    /// at time of export to the source
    /// record to determine if extraction is needed again.
    /// </summary>
    Completed = 2,
}
