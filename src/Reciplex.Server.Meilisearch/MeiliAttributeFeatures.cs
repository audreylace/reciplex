namespace Reciplex.Server.Meilisearch;

public class MeiliAttributeFeatures
{
    public bool FacetSearch { get; init; }
    public MeiliAttributeFilterFlags Filter { get; init; } = new();
}
