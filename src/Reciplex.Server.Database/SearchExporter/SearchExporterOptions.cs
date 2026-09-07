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
}
