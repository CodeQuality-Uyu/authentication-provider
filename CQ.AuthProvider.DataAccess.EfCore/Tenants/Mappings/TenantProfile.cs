﻿using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Abstractions.Tenants;

namespace CQ.AuthProvider.DataAccess.EfCore.Tenants.Mappings;

internal sealed class TenantProfile
    : Profile
{
    public TenantProfile()
    {
        CreateMap<TenantEfCore, Tenant>();
    }
}