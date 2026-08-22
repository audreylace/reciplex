namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// record change action
/// </summary>
[Flags]
public enum RecordChangeActionKind
{
    /// <summary>
    /// kind is created
    /// </summary>
    Created = 1,

    /// <summary>
    /// kind is changed
    /// </summary>
    Changed = 2,

    /// <summary>
    /// kind is deleted
    /// </summary>
    Deleted = 4,
}
