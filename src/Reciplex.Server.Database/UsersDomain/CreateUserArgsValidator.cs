using FluentValidation;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.UsersDomain;

public class CreateUserArgsValidator : AbstractValidator<CreateUserArgs>
{
    public CreateUserArgsValidator()
    {
        RuleFor(args => args.DisplayName)
            .NotEmpty()
            .MaximumLength(UserDbObject.DisplayNameMaxLength);
        RuleFor(args => args.Subject).NotEmpty().MaximumLength(UserDbObject.SubjectMaxLength);
        RuleFor(args => args.Authority).NotEmpty().MaximumLength(UserDbObject.AuthorityMaxLength);
    }
}
