namespace Reciplex.Server.Database.SearchExporter;

record class RecipeDatabaseDeletionRow(
    long RecipeId,
    long SearchEntryId,
    long? NextDeletionTryTime,
    long? DeletionTryCounter
);
