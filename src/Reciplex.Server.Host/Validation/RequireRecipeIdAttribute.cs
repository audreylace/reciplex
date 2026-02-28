namespace Reciplex.Server.Host.Validation;

/// <summary>
/// Validates a recipe ID in a path
/// </summary>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false
)]
public class RequireRecipeIdAttribute : RequiredAndNotEmptyAttribute
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public RequireRecipeIdAttribute()
    {
        ErrorMessage = "A recipe id must be supplied";
    }
}
