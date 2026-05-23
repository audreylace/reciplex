using FluentValidation;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Validates <see cref="CreateRecipeBookArgs"/>
/// </summary>
sealed class CreateRecipeBookArgsValidator : AbstractValidator<CreateRecipeBookArgs>
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public CreateRecipeBookArgsValidator()
    {
        RuleFor(r => r.Name).MaximumLength(RecipeBookDbObject.NameMaxLength).NotEmpty();
        RuleFor(r => r.ShortDescription)
            .MaximumLength(RecipeBookDbObject.ShortDescriptionMaxLength)
            .NotNull();
    }
}
