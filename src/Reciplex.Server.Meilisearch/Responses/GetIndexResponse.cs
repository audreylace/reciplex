namespace Reciplex.Server.Meilisearch.Responses;

public class GetIndexResponse
{
    public required string Uid { get; init; }
    public required string PrimaryKey { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
