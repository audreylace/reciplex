namespace Reciplex.Server.Database.SearchIndexTrackingDomain;

/// <summary>
/// Bridge object for recipe data extracted from the database
/// </summary>
/// <param name="RecipeFk">the recipe foreign key</param>
/// <param name="Name">name of the recipe</param>
/// <param name="ShortDescription">recipe short description</param>
/// <param name="RecipeBookFk">the recipe book foreign key</param>
public sealed record class RecipeRecordDataExtractedFromDatabase(
    long RecipeFk,
    string Name,
    string ShortDescription,
    long RecipeBookFk
);
