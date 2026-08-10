using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Permissions;
using CQ.AuthProvider.BusinessLogic.Roles;
using System.Security.Principal;

namespace CQ.AuthProvider.BusinessLogic.Accounts;

public record class AccountLogged()
    : Account,
    IPrincipal
{
    public string Token { get; init; } = null!;

    /// <summary>
    /// Session the access token belongs to, read from its <c>sid</c> claim.
    /// Empty for subscriptions, which are not backed by a session.
    /// </summary>
    public Guid SessionId { get; init; }

    public List<Guid> AppsIds => Apps.ConvertAll(a => a.Id);

    public List<Guid> RolesIds => Roles.ConvertAll(r => r.Id);

    public List<string> PermissionsKeys => Roles.SelectMany(r => r.Permissions).Select(p => p.Key).ToList();

    public App AppLogged { get; init; } = null!;

    public virtual IIdentity? Identity => null;

    public AccountLogged(
        Account account,
        string token,
        App appLogged)
        : this()
    {
        Id = account.Id;
        Email = account.Email;
        FirstName = account.FirstName;
        LastName = account.LastName;
        FullName = account.FullName;
        ProfilePictureKey = account.ProfilePictureKey;
        Locale = account.Locale;
        TimeZone = account.TimeZone;
        Roles = account.Roles;
        Tenant = account.Tenant;
        Token = token;
        AppLogged = appLogged;
        Apps = account.Apps;
    }

    public static AccountLogged NewSubscription(
        App appLogged,
        string subscription,
        List<Permission> permissions)
    {
        return new AccountLogged
        {
            Tenant = appLogged.Tenant,
            AppLogged = appLogged,
            Token = subscription,
            Roles = [
                    new Role
                    {
                        Permissions = permissions
                    }]
        };
    }


    public bool IsInRole(string role)
    {
        var isRole = Roles.Exists(r =>
        r.Name == role ||
        r.Key == role);

        return isRole || HasPermission(role);
    }

    public bool IsInRole(Guid permissionKey)
    {
        return Roles.Exists(r => r.Id == permissionKey) || HasPermission(permissionKey);
    }
}
