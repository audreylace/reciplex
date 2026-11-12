namespace Reciplex.Server.Meilisearch;

public class SearchByPostArgs
{
    public required string SearchString { get; set; }
    public string? FilterString { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }

    /// <summary>
    /// The list of properties to return from each search document
    /// </summary>
    public List<string>? AttributesToRetrieve { get; set; }
}
