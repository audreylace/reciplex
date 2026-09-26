namespace Reciplex.Server.Database.SearchExporter.Repositories;

sealed record class RecipeRecordDataExtractedFromDatabase(
    long RecipeFk,
    string Name,
    string ShortDescription,
    long RecipeBookFk,
    long SearchVersion
);
