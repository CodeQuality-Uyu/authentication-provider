using CQ.AuthProvider.BusinessLogic.Permissions;
using FluentValidation;

namespace CQ.AuthProvider.BusinessLogic.GoogleAuth;

internal sealed class CreateSessionGoogleArgsValidator
    : AbstractValidator<CreateSessionGoogleArgs>
{
    public CreateSessionGoogleArgsValidator()
    {
        RuleFor(a => a.IdToken)
            .Required();

        RuleFor(a => a.AppId)
            .ValidId();
    }
}
