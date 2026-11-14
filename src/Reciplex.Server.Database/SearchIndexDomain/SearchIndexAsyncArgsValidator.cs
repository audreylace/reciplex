using FluentValidation;
using Reciplex.Server.Database.RecipesDomain;

namespace Reciplex.Server.SearchExporter;

/// <summary>
/// Validates <see cref="SearchRecipesArgs"/>
/// </summary>
sealed class SearchIndexAsyncArgsValidator : AbstractValidator<SearchIndexAsyncArgs>
{
    /// <summary>
    /// Validator
    /// </summary>
    public SearchIndexAsyncArgsValidator()
    {
        RuleFor(r => r.SearchString).MaximumLength(100).NotEmpty();
        RuleFor(r => r.MaxBooks).GreaterThan(0);
        RuleFor(r => r.Limit).GreaterThan(0);
        RuleFor(r => r.Offset).GreaterThanOrEqualTo(0);
        RuleFor(r => r.DocumentKinds).IsInEnum();
    }
}
