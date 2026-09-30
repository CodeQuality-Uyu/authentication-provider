using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Permissions;
using CQ.AuthProvider.BusinessLogic.Roles;

namespace CQ.AuthProvider.Tests;

/// <summary>
/// Alcance de una acción sobre una app que no es la logueada
/// (<see cref="AccountLogged.AssertCanReachApp"/>).
/// </summary>
public sealed class CrossAppReachTests
{
    private const string ChildKey = "deletebyidchildapp-account";

    private const string CrossKey = "deletebyidcrossapp-account";

    private static readonly Guid AppLoggedId = Guid.NewGuid();

    private static readonly Guid TargetAppId = Guid.NewGuid();

    [Fact]
    public void The_logged_app_needs_no_extra_permission()
    {
        var caller = Caller(apps: [], permissions: []);

        caller.AssertCanReachApp(AppLoggedId, isDescendantOfAppLogged: false, ChildKey, CrossKey);
    }

    [Fact]
    public void A_descendant_app_is_reached_with_the_child_app_permission_without_belonging_to_it()
    {
        var caller = Caller(apps: [], permissions: [ChildKey]);

        caller.AssertCanReachApp(TargetAppId, isDescendantOfAppLogged: true, ChildKey, CrossKey);
    }

    [Fact]
    public void A_descendant_app_is_not_reached_without_the_child_app_permission()
    {
        var caller = Caller(apps: [], permissions: [CrossKey]);

        Assert.Throws<CrossAppAccessDeniedException>(
            () => caller.AssertCanReachApp(TargetAppId, isDescendantOfAppLogged: true, ChildKey, CrossKey));
    }

    [Fact]
    public void Another_app_is_reached_with_the_cross_app_permission_when_belonging_to_it()
    {
        var caller = Caller(apps: [TargetAppId], permissions: [CrossKey]);

        caller.AssertCanReachApp(TargetAppId, isDescendantOfAppLogged: false, ChildKey, CrossKey);
    }

    [Fact]
    public void Another_app_is_not_reached_without_belonging_to_it()
    {
        var caller = Caller(apps: [], permissions: [CrossKey, ChildKey]);

        Assert.Throws<CrossAppAccessDeniedException>(
            () => caller.AssertCanReachApp(TargetAppId, isDescendantOfAppLogged: false, ChildKey, CrossKey));
    }

    [Fact]
    public void Another_app_is_not_reached_without_the_cross_app_permission()
    {
        // Pertenecer no alcanza: el permiso de hijas no habilita apps que no descienden.
        var caller = Caller(apps: [TargetAppId], permissions: [ChildKey]);

        Assert.Throws<CrossAppAccessDeniedException>(
            () => caller.AssertCanReachApp(TargetAppId, isDescendantOfAppLogged: false, ChildKey, CrossKey));
    }

    private static AccountLogged Caller(Guid[] apps, string[] permissions)
    {
        return new AccountLogged
        {
            AppLogged = new App { Id = AppLoggedId },
            Apps = [new App { Id = AppLoggedId }, .. apps.Select(id => new App { Id = id })],
            Roles =
            [
                new Role
                {
                    Permissions = [.. permissions.Select(key => new Permission { Key = key })],
                },
            ],
        };
    }
}
