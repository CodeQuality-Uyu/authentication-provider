using CQ.AuthProvider.BusinessLogic.Permissions;
using FluentValidation;

namespace CQ.AuthProvider.BusinessLogic.Roles;

internal sealed class AddAppsArgsValidator
    : AbstractValidator<AddAppsArgs>
{
    public AddAppsArgsValidator()
    {
        RuleFor(x => x.AppIds)
            .Required()
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("AppIds contains duplicate values.");
    }
}
