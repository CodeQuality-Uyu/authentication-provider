using CQ.AuthProvider.BusinessLogic.Permissions;
using FluentValidation;

namespace CQ.AuthProvider.BusinessLogic.Apps;

internal sealed class CreateAppArgsValidator
    : AbstractValidator<CreateAppArgs>
{
    public CreateAppArgsValidator()
    {
        RuleFor(a => a.Name)
            .Required();
    }
}

internal sealed class CreateClientAppArgsValidator
    : AbstractValidator<CreateClientAppArgs>
{
    public CreateClientAppArgsValidator()
    {
        RuleFor(a => a.Name)
            .Required();
    }
}

internal sealed class UpdateAppLogoArgsValidator
    : AbstractValidator<UpdateAppLogoArgs>
{
    public UpdateAppLogoArgsValidator()
    {
        // Sin ninguna key no hay nada que reemplazar: casi seguro es un error del cliente.
        RuleFor(a => a)
            .Must(a =>
                !string.IsNullOrWhiteSpace(a.ColorKey) ||
                !string.IsNullOrWhiteSpace(a.LightKey) ||
                !string.IsNullOrWhiteSpace(a.DarkKey))
            .OverridePropertyName("Logo")
            .WithMessage("At least one logo key is required");
    }
}

internal sealed class LogoValidator
    : AbstractValidator<Logo>
{
    public LogoValidator()
    {
        RuleFor(a => a.ColorKey)
            .Required();

        RuleFor(a => a.LightKey)
            .Required();

        RuleFor(a => a.DarkKey)
            .Required();
    }
}