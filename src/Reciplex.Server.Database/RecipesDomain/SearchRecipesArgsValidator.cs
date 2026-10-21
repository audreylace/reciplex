using FluentValidation;

namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Validates <see cref="SearchRecipesArgs"/>
/// </summary>
sealed class SearchRecipesArgsValidator : AbstractValidator<SearchRecipesArgs>
{
    /// <summary>
    /// Validator
    /// </summary>
    public SearchRecipesArgsValidator()
    {
        RuleFor(r => r.SearchString).MaximumLength(100).NotEmpty();
        RuleFor(r => r.MaxBooks).GreaterThan(0);
        RuleFor(r => r.MaxResults).GreaterThan(0);
        RuleFor(r => r.Books).NotNull();
    }
}
