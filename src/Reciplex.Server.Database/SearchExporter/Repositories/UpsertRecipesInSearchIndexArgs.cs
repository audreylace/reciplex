namespace Reciplex.Server.Database.SearchExporter.Repositories;

sealed class UpsertRecipesInSearchIndexArgs
{
    public ICollection<RecipeSearchDao> Recipes { get; set; } = [];
}
