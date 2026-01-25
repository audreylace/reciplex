using System.ComponentModel.DataAnnotations;

namespace Reciplex.Server.Host.Validation;

/// <summary>
/// Validates that a <see cref="string"/> is supplied and its is not empty or whitespace using <see cref="string.IsNullOrWhiteSpace(string?)"/>
/// </summary>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false
)]
public class RequiredAndNotEmptyAttribute : ValidationAttribute
{
    /// <inheritdoc />
    public override bool IsValid(object? value)
    {
        if (value is not string s)
        {
            return false;
        }
        return !string.IsNullOrWhiteSpace(s);
    }
}
