namespace Reciplex.Server.Host.Options;

/// <summary>
/// Application override metrics
/// </summary>
public class AppMetricsOptions
{
    /// <summary>
    /// Section path
    /// </summary>
    public const string SectionPath = "Reciplex:Metrics";

    /// <summary>
    /// Set to true to enable metric exportation to victoria metrics
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// The path to the victoria metric server
    /// </summary>
    public string ExportUri { get; set; } = "";

    /// <summary>
    /// The name of the service.
    /// </summary>
    public string ServiceName { get; set; } = "Reciplex";
}
