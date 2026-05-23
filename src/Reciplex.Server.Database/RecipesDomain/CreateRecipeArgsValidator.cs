using FluentValidation;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Validates <see cref="CreateRecipeArgs"/>
/// </summary>
sealed class CreateRecipeArgsValidator : AbstractValidator<CreateRecipeArgs>
{
    /// <summary>
    /// Validator
    /// </summary>
    public CreateRecipeArgsValidator()
    {
        RuleFor(r => r.Name).MaximumLength(RecipeDbObject.NameMaxLength).NotEmpty();
        RuleFor(r => r.ShortDescription)
            .MaximumLength(RecipeDbObject.ShortDescriptionMaxLength)
            .NotNull();
        RuleFor(r => r.Details).MaximumLength(RecipeDbObject.DetailsMaxLength).NotNull();
    }
}
