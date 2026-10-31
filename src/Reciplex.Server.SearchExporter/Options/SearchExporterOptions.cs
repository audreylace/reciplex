namespace Reciplex.Server.SearchExporter.Options;

public class SearchExporterOptions
{
    /// <summary>
    /// Section path
    /// </summary>
    public const string SectionPath = "Reciplex:SearchExporter";

    /// <summary>
    /// If the search subsystem is enabled
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// Authentication token for interfacing with the search index
    /// </summary>
    public string AuthenticationToken { get; set; } = "";

    /// <summary>
    /// Search index host
    /// </summary>
    public string Host { get; set; } = "";

    /// <summary>
    /// The max number of records to export to the search index in one batch.
    /// Larger sizes result in more RAM usage on both the service side and
    /// search database but ensure records are synced fasted.
    /// </summary>
    public int SearchExportBatchSize { get; set; } = 20;

    /// <summary>
    /// The max number of records to attempt to delete from the search index in one batch.
    /// Larger sizes result in records getting purged faster from the database
    /// at the expense of more CPU and RAM.
    /// </summary>
    public int SearchDeleteBatchSize { get; set; } = 20;

    /// <summary>
    /// Max time to attempt an export to the search index.
    /// </summary>
    public int MaxExportAttempts { get; set; } = 20;

    /// <summary>
    /// Max times to attempt deleting from the search index before giving up and dropping that
    /// record from the tracking index
    /// </summary>
    public int MaxRecipeDeleteAttempts { get; set; } = 20;

    /// <summary>
    /// The time to sleep before checking for work. Higher frequency
    /// increases database load but reduces the chance that work could get
    /// missed.
    /// </summary>
    public int WorkSleepMs { get; set; } = 1000 * 60 * 10; // 10 minutes

    /// <summary>
    /// The frequency to break leases. Defines how long in MS
    /// to wait between checking for leases to break.
    /// </summary>
    public int LeaseBreakSleepMs { get; set; } = 1000 * 60; // 1 minutes
}
