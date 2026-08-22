namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// record queue change kind
/// </summary>
[Flags]
public enum RecordChangeKind
{
    /// <summary>
    /// change kind is a book
    /// </summary>
    Book = 1,

    /// <summary>
    /// change kind is a recipe
    /// </summary>
    Recipe = 2,
}
