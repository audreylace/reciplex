namespace Reciplex.Server.Host.Models;

/// <summary>
/// pointer to a result from the search index
/// </summary>
/// <param name="Kind">the result kind</param>
/// <param name="DocumentKey">the document key</param>
public record class HitsPtrJson(string Kind, string DocumentKey);
