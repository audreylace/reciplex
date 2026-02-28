using System.ComponentModel.DataAnnotations;

namespace Reciplex.Server.Host.Validation;

/// <summary>
/// Validates that a string is not empty and whitespace when present
/// </summary>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false
)]
public class NotEmptyNorWhitespaceAttribute : ValidationAttribute
{
    /// <inheritdoc />
    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return true;
        }

        if (value is not string s)
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(s);
    }
}
