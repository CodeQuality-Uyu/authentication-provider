using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.DataAccess.EfCore.Permissions;
using CQ.AuthProvider.DataAccess.EfCore.Roles;
using CQ.AuthProvider.DataAccess.EfCore.Tenants;

namespace CQ.AuthProvider.DataAccess.EfCore.Apps;

public sealed record class AppEfCore()
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; init; } = null!;

    public bool IsDefault { get; set; }
    
    public Logo Logo { get; init; } = null!;

    public Background Background { get; init; } = null!;

    public Guid TenantId { get; init; }

    public TenantEfCore Tenant { get; init; } = null!;

    public Guid? FatherAppId { get; init; }

    public AppEfCore? FatherApp { get; init; }

    public List<RoleEfCore> Roles { get; init; } = [];

    public List<PermissionEfCore> Permissions { get; init; } = [];
}
