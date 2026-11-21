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

    /// <summary>
    /// Set of attributes to search on. If not specified, all attributes in the document are searched including
    /// internal meta data properties.
    /// </summary>
    public List<string>? AttributesToSearchOn { get; set; }
}
