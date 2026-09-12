namespace Reciplex.Server.Database.SearchExporter.Repositories;

class UpsertRecipesInSearchIndexArgs
{
    public ICollection<RecipeSearchIndexInformation> Recipes { get; set; } = [];
}
