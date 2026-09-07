namespace Reciplex.Server.Database.SearchExporter;

record class RecipeRecordDataExtractedFromDatabase(
    long RecipeFk,
    string Name,
    string ShortDescription,
    long RecipeBookFk,
    long SearchVersion
);
