using CQ.AuthProvider.BusinessLogic.Permissions;
using FluentValidation;

namespace CQ.AuthProvider.BusinessLogic.EmailVerifications;

internal sealed class CreateEmailVerificationArgsValidator
    : AbstractValidator<CreateEmailVerificationArgs>
{
    public CreateEmailVerificationArgsValidator()
    {
        RuleFor(c => c.Email)
            .RequiredEmail();
    }
}
