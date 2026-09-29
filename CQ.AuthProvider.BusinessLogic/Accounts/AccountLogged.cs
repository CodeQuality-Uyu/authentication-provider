using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Permissions;
using CQ.AuthProvider.BusinessLogic.Roles;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.AuthProvider.BusinessLogic.Utils;
using System.Security.Principal;

namespace CQ.AuthProvider.BusinessLogic.Accounts;

public record class AccountLogged()
    : Account,
    IPrincipal
{
    public string Token { get; init; } = null!;

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

    /// <summary>
    /// Si la cuenta tiene la vista global (<c>getallcrosstenant-account</c>): puede leer datos de
    /// cualquier tenant.
    /// </summary>
    public bool HasCrossTenantView()
    {
        return HasPermission(AuthConstants.GET_ALL_CROSS_TENANT_ACCOUNT_PERMISSION_KEY);
    }

    /// <summary>
    /// Tenant por el que filtrar una lectura, a partir del <paramref name="tenantId"/> pedido.
    /// </summary>
    /// <returns>
    /// Con la vista global, el pedido tal cual: <c>null</c> significa <b>todos los tenants</b>.
    /// Sin ella, siempre el tenant de la cuenta.
    /// </returns>
    /// <exception cref="CrossTenantAccessDeniedException">
    /// Sin la vista global, si se pide un tenant que no es el de la cuenta.
    /// </exception>
    public Guid? ResolveTenantFilter(Guid? tenantId)
    {
        if (HasCrossTenantView())
        {
            return tenantId;
        }

        if (tenantId.HasValue && tenantId.Value != Tenant.Id)
        {
            throw new CrossTenantAccessDeniedException(tenantId.Value);
        }

        return Tenant.Id;
    }
}
