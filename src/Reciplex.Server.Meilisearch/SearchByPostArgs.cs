namespace Reciplex.Server.Meilisearch;

public class SearchByPostArgs
{
    public required string SearchString { get; set; }
    public string? FilterString { get; set; }
}
