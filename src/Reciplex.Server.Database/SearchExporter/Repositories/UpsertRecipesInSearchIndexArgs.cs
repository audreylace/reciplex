namespace Reciplex.Server.Database.SearchExporter.Repositories;

sealed class UpsertRecipesInSearchIndexArgs
{
    public ICollection<RecipeSearchIndexInformation> Recipes { get; set; } = [];
}
