using CQ.AuthProvider.BusinessLogic.Permissions;
using FluentValidation;

namespace CQ.AuthProvider.BusinessLogic.ResetPasswords;

internal sealed class VerifyResetPasswordArgsValidator
    : AbstractValidator<VerifyResetPasswordArgs>
{
    public VerifyResetPasswordArgsValidator()
    {
        RuleFor(r => r.Email)
            .RequiredEmail();

        RuleFor(r => r.Code)
            .Required()
            .InclusiveBetween(100000, 999999);
    }
}
