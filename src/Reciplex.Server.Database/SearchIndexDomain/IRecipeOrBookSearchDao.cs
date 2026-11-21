namespace Reciplex.Server.Database.SearchIndexDomain;

/// <summary>
/// Tag for a dao that represents a recipe or book
/// </summary>
public interface IRecipeOrBookSearchDao
{
    /// <summary>
    /// The source row id
    /// </summary>
    long RowId { get; }
}
