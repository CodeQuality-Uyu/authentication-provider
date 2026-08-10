using CQ.AuthProvider.BusinessLogic.Apps;
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
            invitation.App);

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
