using CQ.AuthProvider.BusinessLogic.Permissions;
using FluentValidation;

namespace CQ.AuthProvider.BusinessLogic.Accounts;

internal sealed class CreateAccountArgsValidator
    : AbstractValidator<CreateAccountArgs>
{
    public CreateAccountArgsValidator()
    {
        RuleFor(a => a.FirstName)
            .Required();

        RuleFor(a => a.LastName)
            .Required();

        RuleFor(a => a.Locale)
            .Required();

        RuleFor(a => a.TimeZone)
            .Required();

        RuleFor(a => a.Email)
            .RequiredEmail();

        RuleFor(a => a.Password)
            .RequiredPassword()
            .When(a => !a.IsPasswordHashed);

        RuleFor(a => a.AppId)
            .ValidId();

        RuleFor(a => a.RoleId)
            .ValidId();

        RuleFor(a => a)
            .Must(a => !string.IsNullOrEmpty(a.VerificationToken) || a.VerificationCode.HasValue)
            .WithMessage("Either VerificationToken or VerificationCode must be provided");
    }
}

internal sealed class CreateAccountForArgsValidator
    : AbstractValidator<CreateAccountForArgs>
{
    public CreateAccountForArgsValidator()
    {
        RuleFor(a => a.FirstName)
            .Required();

        RuleFor(a => a.LastName)
            .Required();

        RuleFor(a => a.Locale)
            .Required();

        RuleFor(a => a.TimeZone)
            .Required();

        RuleFor(a => a.Email)
            .RequiredEmail();

        RuleFor(a => a.RoleIds)
            .ValidIds();

        RuleFor(a => a.AppIds)
            .ValidIds();
    }
}

internal sealed class CreateAccountWithTenantArgsValidator
    : AbstractValidator<CreateAccountWithTenantArgs>
{
    public CreateAccountWithTenantArgsValidator()
    {
        RuleFor(a => a.FirstName)
            .Required();

        RuleFor(a => a.LastName)
            .Required();

        RuleFor(a => a.Locale)
            .Required();

        RuleFor(a => a.TimeZone)
            .Required();

        RuleFor(a => a.Email)
            .RequiredEmail();

        RuleFor(a => a.Password)
            .RequiredPassword();

        RuleFor(a => a.TenantName)
            .Required();
    }
}
