using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.DataAccess.EfCore.Apps;
using CQ.AuthProvider.DataAccess.EfCore.Roles;
using CQ.AuthProvider.DataAccess.EfCore.Tenants;

namespace CQ.AuthProvider.DataAccess.EfCore.Accounts;

public sealed record class AccountEfCore()
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Email { get; init; } = null!;

    public string? ProfilePictureKey { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Locale { get; set; } = null!;

    public string TimeZone { get; set; } = null!;

    public bool IsEmailVerified { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow.Date;

    public Guid TenantId { get; set; }

    public TenantEfCore Tenant { get; init; } = null!;

    public List<RoleEfCore> Roles { get; init; } = [];

    public List<AppEfCore> Apps { get; init; } = [];

    internal static AccountEfCore New(Account account) => new()
    {
        Id = account.Id,
        Email = account.Email,
        FullName = account.FullName,
        FirstName = account.FirstName,
        LastName = account.LastName,
        Locale = account.Locale,
        TimeZone = account.TimeZone,
        ProfilePictureKey = account.ProfilePictureKey,
        TenantId = account.Tenant.Id,
    };
}
