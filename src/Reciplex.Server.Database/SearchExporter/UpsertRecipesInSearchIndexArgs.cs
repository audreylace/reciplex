namespace Reciplex.Server.Database.SearchExporter;

class UpsertRecipesInSearchIndexArgs
{
    public ICollection<RecipeSearchIndexInformation> Recipes { get; set; } = [];
}
