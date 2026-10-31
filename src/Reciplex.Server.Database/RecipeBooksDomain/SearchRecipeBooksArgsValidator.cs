using FluentValidation;

namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Validates <see cref="SearchRecipesArgs"/>
/// </summary>
sealed class SearchRecipeBooksArgsValidator : AbstractValidator<SearchBooksArgs>
{
    /// <summary>
    /// Validator
    /// </summary>
    public SearchRecipeBooksArgsValidator()
    {
        RuleFor(r => r.SearchString).MaximumLength(100).NotEmpty();
        RuleFor(r => r.MaxBooks).GreaterThan(0);
        RuleFor(r => r.Limit).GreaterThan(0);
        RuleFor(r => r.Offset).GreaterThanOrEqualTo(0);
    }
}
