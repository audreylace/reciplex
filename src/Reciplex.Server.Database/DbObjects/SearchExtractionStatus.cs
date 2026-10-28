namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Search extraction status. Controls what operation should run.
/// </summary>
public enum SearchExtractionStatus
{
    /// <summary>
    /// Data needs extraction
    /// </summary>
    PendingExtraction = 1,

    /// <summary>
    /// Data is extracted
    /// </summary>
    Extracted = 2,

    /// <summary>
    /// Data requires deletion
    /// </summary>
    PendingDelete = 3,
}
