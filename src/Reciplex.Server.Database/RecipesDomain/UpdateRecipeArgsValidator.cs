using FluentValidation;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipesDomain;

/// <summary>
/// Validates a <see cref="UpdateRecipeArgs"/>
/// </summary>
sealed class UpdateRecipeArgsValidator : AbstractValidator<UpdateRecipeArgs>
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdateRecipeArgsValidator()
    {
        RuleFor(r => r.Name).MaximumLength(RecipeDbObject.NameMaxLength).NotEmpty();
        RuleFor(r => r.ShortDescription)
            .MaximumLength(RecipeDbObject.ShortDescriptionMaxLength)
            .NotNull();
        RuleFor(r => r.Details).MaximumLength(RecipeDbObject.DetailsMaxLength).NotNull();
    }
}
