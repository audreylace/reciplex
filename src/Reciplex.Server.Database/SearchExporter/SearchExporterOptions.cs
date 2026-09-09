namespace Reciplex.Server.Database.SearchExporter;

public class SearchExporterOptions
{
    /// <summary>
    /// Section path
    /// </summary>
    public const string SectionPath = "Reciplex:SearchExporter";
    public bool Enable { get; set; }
    public string AuthenticationToken { get; set; } = "";
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
    /// Number of milliseconds to pause after extracting data from the database and
    /// exporting it to the search index. Delay exists to ensure the search
    /// export process does not fully consume the database resources. Lower
    /// values combined with <see cref="SearchExportBatchSize" /> export faster
    /// at the cost of more RAM, CPU, and database contention.
    /// </summary>
    public int SearchExportSuccessPauseMs { get; set; } = 20;

    public int SearchExportDeleteLoopPauseMs { get; set; } = 50;

    /// <summary>
    /// How long to wait between each lease break loop
    /// </summary>
    public int LeaseBreakLoopPauseMs { get; set; } = 50;

    /// <summary>
    /// Max number of lease records to break in one batch
    /// </summary>
    public int LeaseBreakBatchSize { get; set; } = 100;
}
