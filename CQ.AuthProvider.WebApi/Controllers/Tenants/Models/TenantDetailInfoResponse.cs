﻿namespace CQ.AuthProvider.WebApi.Controllers.Tenants.Models;

public readonly struct TenantDetailInfoResponse
{
    public string Id { get; init; }

    public string Name { get; init; }

    public AccountBasicInfoResponse Owner { get; init; }
}