using CQ.AuthProvider.BusinessLogic.Permissions;
using FluentValidation;

namespace CQ.AuthProvider.BusinessLogic.EmailVerifications;

internal sealed class AcceptEmailVerificationArgsValidator
    : AbstractValidator<AcceptEmailVerificationArgs>
{
    public AcceptEmailVerificationArgsValidator()
    {
        RuleFor(a => a.Email)
            .RequiredEmail();

        RuleFor(a => a.Token)
            .NotEmpty()
            .WithMessage("Can't be empty")
            .When(a => a.Token is not null);

        RuleFor(a => a.Code)
            .InclusiveBetween(100000, 999999)
            .When(a => a.Code.HasValue);

        RuleFor(a => a)
            .Must(a => !string.IsNullOrEmpty(a.Token) || a.Code.HasValue)
            .WithMessage("Either Token or Code must be provided");
    }
}
