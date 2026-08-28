namespace Reciplex.Server.Database.SearchExporter;

record class RecipeDatabaseExtractionRow(
    long RecipeId,
    long SearchEntryId,
    long RecipeBookFk,
    string RecipeName,
    string RecipeDescription,
    long SearchVersion,
    long ErrorCount,
    long? ErrorVersion
);
