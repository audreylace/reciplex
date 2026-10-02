namespace Reciplex.Server.Database.SearchExporter.Repositories;

sealed class UpsertRecipesInSearchIndexArgs
{
    public ICollection<RecipeSearchIndexDocument> Recipes { get; set; } = [];
}
