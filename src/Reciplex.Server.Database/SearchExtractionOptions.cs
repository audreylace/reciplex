namespace Reciplex.Server.Database;

/// <summary>
/// Search extraction options for the database
/// </summary>
public class SearchExtractionOptions
{
    /// <summary>
    /// Section path
    /// </summary>
    public const string SectionPath = "Reciplex:SearchExtraction";

    /// <summary>
    /// If the search driver is enabled
    /// </summary>
    public bool Enable { get; set; }
}
