using CQ.AuthProvider.BusinessLogic.Abstractions.Apps;
using CQ.AuthProvider.BusinessLogic.Abstractions.Roles;
using CQ.AuthProvider.BusinessLogic.Abstractions.Tenants;
using CQ.Exceptions;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.Abstractions.Accounts;

public record class Account()
{
    public string Id { get; init; } = Db.NewId();

    public string Email { get; init; } = null!;

    public string FirstName { get; init; } = null!;

    public string LastName { get; init; } = null!;

    public string FullName { get; init; } = null!;

    public string? ProfilePictureId { get; init; } = null!;

    public string Locale { get; init; } = null!;

    public string TimeZone { get; init; } = null!;

    public List<Role> Roles { get; init; } = [];

    public List<App> Apps { get; init; } = [];

    public Tenant Tenant { get; set; } = null!;

    // For new Account
    public Account(
        string email,
        string firstName,
        string lastName,
        string? profilePictureUrl,
        string locale,
        string timeZone,
        Role role,
        App app)
        : this()
    {
        Email = email;
        FirstName = Guard.Normalize(firstName);
        LastName = Guard.Normalize(lastName);
        FullName = $"{FirstName} {LastName}";
        ProfilePictureId = profilePictureUrl;
        Locale = locale;
        TimeZone = timeZone;
        Roles = [role];
        Tenant = app.Tenant;
        Apps = [app];
    }

    public void AssertPermission(string permissionKey)
    {
        var missingPermission = !HasPermission(permissionKey);

        if (missingPermission)
        {
            throw new AccessDeniedException(permissionKey.ToString());
        }
    }

    public bool HasPermission(string permissionKey)
    {
        var allPermissions = Roles
           .SelectMany(r => r.Permissions)
           .ToList();

        var hasPermission = allPermissions.Exists(p => p.HasPermissionKey(permissionKey));

        return hasPermission;
    }
}
