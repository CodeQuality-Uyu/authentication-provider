using FluentValidation;

namespace CQ.AuthProvider.BusinessLogic.Sessions;

internal sealed class RefreshSessionArgsValidator
    : AbstractValidator<RefreshSessionArgs>
{
    public RefreshSessionArgsValidator()
    {
        RuleFor(a => a.RefreshToken)
            .NotEmpty();
    }
}
