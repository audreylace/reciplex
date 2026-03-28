using FluentValidation;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipeBooksDomain;

public class UpdateRecipeBookArgsValidator : AbstractValidator<UpdateRecipeBookArgs>
{
    public UpdateRecipeBookArgsValidator()
    {
        RuleFor(r => r.Name).MaximumLength(RecipeBookDbObject.NameMaxLength).NotEmpty();
        RuleFor(r => r.ShortDescription)
            .MaximumLength(RecipeBookDbObject.ShortDescriptionMaxLength)
            .NotNull();
        RuleFor(r => r.ConcurrencyTag).NotEmpty();
    }
}
