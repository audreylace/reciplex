namespace Reciplex.Server.Database.SearchExporter;

sealed record class RecipeRecordDataExtractedFromDatabase(
    long RecipeFk,
    string Name,
    string ShortDescription,
    long RecipeBookFk,
    long SearchVersion
);
