namespace Reciplex.Server.Database.SearchIndexTrackingDomain;

/// <summary>
/// Bridge object for book data extracted from the database
/// </summary>
/// <param name="RowId">the source row Id</param>
/// <param name="Name">name of the recipe</param>
/// <param name="ShortDescription">recipe short description</param>
/// <param name="RecipeBookFk">the recipe book foreign key</param>
public sealed record class BookRecordDataExtractedFromDatabase(
    long RowId,
    string Name,
    string ShortDescription,
    long RecipeBookFk
) : IExtractedSearchRow;
