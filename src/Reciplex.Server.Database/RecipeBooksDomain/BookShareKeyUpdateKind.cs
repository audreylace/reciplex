namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Type of share key update
/// </summary>
public enum BookShareKeyUpdateKind
{
    /// <summary>
    /// Clear the share key
    /// </summary>
    Clear,

    /// <summary>
    /// Generate a new share key
    /// </summary>
    Regenerate,
}
