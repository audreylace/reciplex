namespace Reciplex.Server.Host.Validation;

/// <summary>
/// Validates an <c>If-Match</c> header
/// </summary>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false
)]
public class RequiredAndValidIfMatchAttribute : RequiredAndNotEmptyAttribute
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public RequiredAndValidIfMatchAttribute()
    {
        ErrorMessage =
            "An If-Match header that is not null, not empty, and not whitespace, must be supplied.";
    }
}
