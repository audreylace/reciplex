using FluentValidation;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipesDomain;

public class CreateRecipeArgsValidator : AbstractValidator<CreateRecipeArgs>
{
    public CreateRecipeArgsValidator()
    {
        RuleFor(r => r.Name).MaximumLength(RecipeDbObject.NameMaxLength).NotEmpty();
        RuleFor(r => r.ShortDescription)
            .MaximumLength(RecipeDbObject.ShortDescriptionMaxLength)
            .NotNull();
        RuleFor(r => r.Details).MaximumLength(RecipeDbObject.DetailsMaxLength).NotNull();
    }
}
