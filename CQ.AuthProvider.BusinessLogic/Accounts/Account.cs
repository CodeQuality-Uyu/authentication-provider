using CQ.AuthProvider.BusinessLogic.Accounts.Exceptions;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.GoogleAuth.Exceptions;
using CQ.AuthProvider.BusinessLogic.Invitations;
using CQ.AuthProvider.BusinessLogic.Roles;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.Accounts;

public record class Account()
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Email { get; init; } = null!;

    public string FirstName { get; init; } = null!;

    public string LastName { get; init; } = null!;

    public string FullName { get; init; } = null!;

    public string? ProfilePictureKey { get; init; } = null!;

    public string Locale { get; init; } = null!;

    public string TimeZone { get; init; } = null!;

    public bool IsEmailVerified { get; init; }

    public List<Role> Roles { get; init; } = [];

    public List<App> Apps { get; init; } = [];

    public Tenant Tenant { get; init; } = null!;

    public static Account New(
        string email,
        string firstName,
        string lastName,
        string? profilePictureKey,
        string locale,
        string timeZone,
        Role role,
        App app)
    {
        firstName = Guard.Normalize(firstName);
        lastName = Guard.Normalize(lastName);

        return new Account
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            FullName = $"{firstName} {lastName}",
            ProfilePictureKey = profilePictureKey,
            Locale = locale,
            TimeZone = timeZone,
            Roles = [role],
            Tenant = app.Tenant,
            Apps = [app]
        };
    }

    public static Account New(
        string email,
        string firstName,
        string lastName,
        string? profilePictureKey,
        string locale,
        string timeZone,
        List<Role> roles,
        List<App> apps,
        Tenant tenant)
    {
        firstName = Guard.Normalize(firstName);
        lastName = Guard.Normalize(lastName);

        return new Account
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            FullName = $"{firstName} {lastName}",
            ProfilePictureKey = profilePictureKey,
            Locale = locale,
            TimeZone = timeZone,
            Roles = roles,
            Tenant = tenant,
            Apps = apps
        };
    }

    public static Account NewFromInvitation(
        string email,
        string firstName,
        string lastName,
        string? profilePictureKey,
        string locale,
        string timeZone,
        Invitation invitation) => New(email,
            firstName,
            lastName,
            profilePictureKey,
            locale,
            timeZone,
            invitation.Role,
            invitation.App)
            with
            { IsEmailVerified = true };

    public static Account NewWithTenant(
        string email,
        string firstName,
        string lastName,
        string? profilePictureKey,
        string locale,
        string timeZone,
        Tenant tenant)
    {
        firstName = Guard.Normalize(firstName);
        lastName = Guard.Normalize(lastName);

        return new Account
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            FullName = $"{firstName} {lastName}",
            ProfilePictureKey = profilePictureKey,
            Locale = locale,
            TimeZone = timeZone,
            Roles = [
                new Role
                {
                    Id = AuthConstants.APP_OWNER_ROLE_ID,
                    Name = AuthConstants.APP_OWNER_ROLE_NAME,
                },
                new Role
                {
                    Id = AuthConstants.TENANT_OWNER_ROLE_ID,
                    Name = AuthConstants.TENANT_OWNER_ROLE_NAME,
                }
            ],
            Tenant = tenant,
            Apps = [
                new App
                {
                    Id = AuthConstants.AUTH_WEB_API_APP_ID,
                    Name = AuthConstants.AUTH_WEB_API_APP_NAME,
                }
            ]
        };
    }

    /// <summary>
    /// Qué implica sacar la cuenta del app <paramref name="appId"/>. Es la regla común de
    /// <c>DELETE /me</c> (la cuenta se saca a sí misma) y <c>DELETE /accounts/{id}</c> (un admin
    /// saca a otra).
    /// </summary>
    /// <returns>
    /// <see cref="AppRemoval.RemoveApp"/> si la cuenta usa otras apps: el mismo email puede estar
    /// en varias y solo se la saca de esta. <see cref="AppRemoval.DeleteAccount"/> si era su única
    /// app: se borra entera para que el email quede libre.
    /// </returns>
    /// <exception cref="AccountDeletionNotAllowedException">
    /// Si <paramref name="appId"/> es la consola del Auth Provider: ahí se administran tenants y
    /// apps, y sacar la cuenta podría dejarlos huérfanos.
    /// </exception>
    /// <exception cref="AccountNotInAppException">Si la cuenta no pertenece al app.</exception>
    public AppRemoval ResolveRemovalFrom(Guid appId)
    {
        if (appId == AuthConstants.AUTH_WEB_API_APP_ID)
        {
            throw new AccountDeletionNotAllowedException(Email, appId);
        }

        if (!Apps.Exists(a => a.Id == appId))
        {
            throw new AccountNotInAppException(Email, appId);
        }

        var belongsToOtherApps = Apps.Exists(a => a.Id != appId);

        return belongsToOtherApps
            ? AppRemoval.RemoveApp
            : AppRemoval.DeleteAccount;
    }

    public bool HasPermission(string permissionKey)
    {
        return CheckPermission(permissionKey);
    }

    public bool HasPermission(Guid permissionId)
    {
        return Roles.Exists(r => r.Id == permissionId);
    }

    private bool CheckPermission(string permissionKey)
    {
        var allPermissions = Roles
            .SelectMany(r => r.Permissions)
            .ToList();

        var hasPermission = allPermissions.Exists(p => p.HasPermissionKey(permissionKey));

        return hasPermission;
    }
}
