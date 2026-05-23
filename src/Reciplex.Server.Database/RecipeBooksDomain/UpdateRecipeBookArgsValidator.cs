using FluentValidation;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipeBooksDomain;

/// <summary>
/// Validates <see cref="UpdateRecipeBookArgs"/>
/// </summary>
sealed class UpdateRecipeBookArgsValidator : AbstractValidator<UpdateRecipeBookArgs>
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdateRecipeBookArgsValidator()
    {
        RuleFor(r => r.Name).MaximumLength(RecipeBookDbObject.NameMaxLength).NotEmpty();
        RuleFor(r => r.ShortDescription)
            .MaximumLength(RecipeBookDbObject.ShortDescriptionMaxLength)
            .NotNull();
        RuleFor(r => r.ConcurrencyTag).NotEmpty();
    }
}
