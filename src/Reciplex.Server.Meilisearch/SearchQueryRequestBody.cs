using System.Text.Json.Serialization;

namespace Reciplex.Server.Meilisearch;

/// <summary>
/// JSON body for the search by post endpoint of Meilisearch
/// <br />
/// <code>
/// POST /indexes/{index_uid}/search
/// {
///   q = "search string",
///   filter = "... filter string ...",
///   offset = 33,
///   limit = 14
/// }
/// </code>
/// </summary>
sealed class SearchQueryRequestBody
{
    /// <summary>
    /// search string for the index
    /// </summary>
    [JsonPropertyName("q")]
    public required string SearchString { get; init; }

    /// <summary>
    /// Filter condition of the query
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Filter { get; init; }

    /// <summary>
    /// Result offset
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Offset { get; init; }

    /// <summary>
    /// Number of results to return in this page
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Limit { get; init; }

    /// <summary>
    /// The list of properties to return from each search document
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? AttributesToRetrieve { get; set; }

    /// <summary>
    /// Set of attributes to search on. If not specified, all attributes in the document are searched including
    /// internal meta data properties.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? AttributesToSearchOn { get; set; }
}
