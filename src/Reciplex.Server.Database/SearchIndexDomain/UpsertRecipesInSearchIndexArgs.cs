namespace Reciplex.Server.Database.SearchIndexDomain;

public sealed class UpsertRecipesInSearchIndexArgs
{
    public ICollection<RecipeSearchDao> Recipes { get; set; } = [];
}
