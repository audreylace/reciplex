namespace Reciplex.Server.Database.SearchIndexTrackingDomain;

public sealed record class RecipeRecordDataExtractedFromDatabase(
    long RecipeFk,
    string Name,
    string ShortDescription,
    long RecipeBookFk
);
