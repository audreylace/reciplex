namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// record queue change source kind
/// </summary>
[Flags]
public enum RecordChangeSourceKind
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
