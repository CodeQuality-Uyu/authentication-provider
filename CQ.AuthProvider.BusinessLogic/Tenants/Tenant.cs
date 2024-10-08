﻿using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.Tenants;

public sealed record class Tenant()
{
    public string Id { get; init; } = Db.NewId();

    public string Name { get; init; } = null!;

    public Account Owner { get; init; } = null!;

    // For new Tenant
    public Tenant(
        string name,
        Account owner)
        : this()
    {
        Name = name;
        Owner = owner;
    }
}
