﻿using CQ.ApiElements;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.Utility;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Interceptors;

namespace CQ.AuthProvider.BusinessLogic.Permissions;
internal sealed class CreatePermissionArgsValidator
    : AbstractValidator<CreatePermissionArgs>,
    IValidatorInterceptor
{
    public CreatePermissionArgsValidator()
    {
        RuleFor(a => a.Name)
            .RequiredString();

        RuleFor(a => a.Description)
            .RequiredString();

        RuleFor(a => a.Key)
            .RequiredString();

        RuleFor(a => a.AppId)
            .ValidId();
    }

    public ValidationResult? AfterValidation(
        ActionExecutingContext actionExecutingContext,
        IValidationContext validationContext)
    {
        var validationResult = new ValidationResult();
        var accountLogged = (AccountLogged)actionExecutingContext.HttpContext.Items[ContextItem.AccountLogged];

        var args = (CreatePermissionArgs)validationContext.InstanceToValidate;

        if (Guard.IsNull(args.AppId) && accountLogged.AppLogged.Id == AuthConstants.AUTH_WEB_API_APP_ID)
        {
            validationResult.Errors.Add(new ValidationFailure("AppId", "Can't create to auth api app"));
        }

        return validationResult;
    }

    public IValidationContext? BeforeValidation(ActionExecutingContext actionExecutingContext, IValidationContext validationContext)
    {
        return null;
    }
}

internal static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> RequiredString<T>(this IRuleBuilder<T, string> validator)
    {
        var options = validator
            .Required()
            .NotEmpty().WithMessage("Can't be empty");

        return options;
    }

    public static IRuleBuilderOptions<T, TProp> Required<T, TProp>(this IRuleBuilder<T, TProp> validator)
    {
        var options = validator
            .NotNull()
            .WithMessage("Can't be null");

        return options;
    }

    public static IRuleBuilderOptions<T, Guid?> ValidId<T>(this IRuleBuilder<T, Guid?> validator)
    {
        var options = validator
            .Must((id) =>
            {
                if (Guard.IsNull(id))
                {
                    return true;
                }

                return id != Guid.Empty;
            })
            .WithMessage("Invalid id");

        return options;
    }

    public static IRuleBuilderOptions<T, Guid> ValidId<T>(this IRuleBuilder<T, Guid> validator)
    {
        var options = validator
            .Must((id) => id != Guid.Empty)
            .WithMessage("Invalid id");

        return options;
    }

    public static IRuleBuilderOptions<T, string?> RequiredEmail<T>(this IRuleBuilder<T, string?> validator)
    {
        var options = validator
            .RequiredString()
            .EmailAddress()
            .WithMessage("Invalid format");

        return options;
    }

    public static IRuleBuilderOptions<T, string?> RequiredPassword<T>(this IRuleBuilder<T, string?> validator)
    {
        var options = validator
            .RequiredString()
            .MinimumLength(6)
            .WithMessage("Minimum 6 characters")
            .Must(password =>
            {
                try
                {
                    Guard.ThrowIsInputInvalidPassword(password);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            })
            .WithMessage("Invalid, must have a special character");

        return options;
    }
}