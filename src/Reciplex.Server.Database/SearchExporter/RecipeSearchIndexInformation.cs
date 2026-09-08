namespace Reciplex.Server.Database.SearchExporter;

class RecipeSearchIndexInformation
{
    public required long RecipeId { get; init; }
    public required string Name { get; init; }
    public required string ShortDescription { get; init; }
    public required long RecipeBookId { get; init; }
}
