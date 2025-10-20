﻿using CQ.AuthProvider.BusinessLogic.Permissions;
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
            .RequiredPassword();

        RuleFor(a => a.AppId)
            .ValidId();
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

        RuleFor(a => a.Password)
            .RequiredPassword();

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
