﻿using CQ.AuthProvider.DataAccess.EfCore.Permissions;
using CQ.AuthProvider.DataAccess.EfCore.Roles;
using CQ.AuthProvider.DataAccess.EfCore.Tenants;

namespace CQ.AuthProvider.DataAccess.EfCore.Apps;

public sealed record class AppEfCore()
{
    public string Id { get; init; } = null!;

    public string Name { get; init; } = null!;

    public bool IsDefault { get; init; }

    public string TenantId { get; init; } = null!;

    public TenantEfCore Tenant { get; init; } = null!;

    public List<RoleEfCore> Roles { get; init; } = [];

    public List<PermissionEfCore> Permissions { get; init; } = [];
}
