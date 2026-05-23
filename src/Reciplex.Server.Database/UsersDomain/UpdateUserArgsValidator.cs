using FluentValidation;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.UsersDomain;

sealed class UpdateUserArgsValidator : AbstractValidator<UpdateUserArgs>
{
    public UpdateUserArgsValidator()
    {
        RuleFor(args => args.DisplayName)
            .NotEmpty()
            .MaximumLength(UserDbObject.DisplayNameMaxLength);
    }
}
