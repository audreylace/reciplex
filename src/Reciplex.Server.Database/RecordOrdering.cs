namespace Reciplex.Server.Database;

/// <summary>
/// How to order records
/// </summary>
public enum RecordOrdering
{
    /// <summary>
    /// order by id increasing. Lower id values come first.
    /// </summary>
    ByIdIncreasing,

    /// <summary>
    /// order by id decreasing. Higher id values come first.
    /// </summary>
    ByIdDecreasing,
}
