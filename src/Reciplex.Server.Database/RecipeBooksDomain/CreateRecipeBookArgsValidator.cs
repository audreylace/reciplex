using FluentValidation;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipeBooksDomain;

public class CreateRecipeBookArgsValidator : AbstractValidator<CreateRecipeBookArgs>
{
    public CreateRecipeBookArgsValidator()
    {
        RuleFor(r => r.Name).MaximumLength(RecipeBookDbObject.NameMaxLength).NotEmpty();
        RuleFor(r => r.ShortDescription)
            .MaximumLength(RecipeBookDbObject.ShortDescriptionMaxLength)
            .NotNull();
    }
}
