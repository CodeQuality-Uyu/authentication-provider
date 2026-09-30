using CQ.AuthProvider.BusinessLogic.Apps;
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
    /// Alcance de una acción sobre <paramref name="appId"/>, dentro del tenant de la sesión.
    /// </summary>
    /// <remarks>
    /// Por defecto una acción alcanza solo la app logueada. Para ir más allá hay dos vías, cada una
    /// con su permiso propio:
    /// <list type="bullet">
    /// <item><b>App descendiente</b> de la logueada: alcanza con
    /// <paramref name="childAppPermissionKey"/>, sin pertenecer a ella. La jerarquía es la que
    /// da el alcance.</item>
    /// <item><b>Cualquier otra app</b> del tenant: hace falta
    /// <paramref name="crossAppPermissionKey"/> <b>y</b> pertenecer a esa app.</item>
    /// </list>
    /// El tenant no se cruza nunca: los descendientes y las apps de la cuenta son del mismo tenant
    /// por construcción, y quien llama igual mantiene su filtro por tenant.
    /// </remarks>
    /// <param name="isDescendantOfAppLogged">
    /// Si <paramref name="appId"/> desciende de <see cref="AppLogged"/>. Lo resuelve quien llama
    /// contra el árbol de apps; acá no hay acceso a datos.
    /// </param>
    /// <exception cref="CrossAppAccessDeniedException">Si ninguna vía aplica.</exception>
    public void AssertCanReachApp(
        Guid appId,
        bool isDescendantOfAppLogged,
        string childAppPermissionKey,
        string crossAppPermissionKey)
    {
        if (appId == AppLogged.Id)
        {
            return;
        }

        if (isDescendantOfAppLogged && HasPermission(childAppPermissionKey))
        {
            return;
        }

        var belongsToApp = AppsIds.Contains(appId);
        if (belongsToApp && HasPermission(crossAppPermissionKey))
        {
            return;
        }

        var requirement = isDescendantOfAppLogged
            ? $"the permission {childAppPermissionKey}"
            : $"to belong to the app and the permission {crossAppPermissionKey}";

        throw new CrossAppAccessDeniedException(appId, requirement);
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
