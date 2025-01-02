﻿using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.AuthProvider.BusinessLogic.Utils;

namespace CQ.AuthProvider.DataAccess.EfCore.Accounts;

internal sealed class AccountProfile
    : Profile
{
    public AccountProfile()
    {
        CreateMap<AccountEfCore, Account>()
            .ForMember(destination => destination.Tenant,
            options => options.MapFrom<TenantValueResolver>())

            .ForMember(destination => destination.Apps,
            options => options.MapFrom(
                source => source.Apps.ConvertAll(a => new App
                {
                    Id = a.Id,
                    IsDefault = a.IsDefault,
                })));
        this.CreateOnlyPaginationMap<AccountEfCore, Account>();
    }
}

internal sealed class TenantValueResolver
    : IValueResolver<AccountEfCore, Account, Tenant>
{
    public Tenant Resolve(
        AccountEfCore source,
        Account destination,
        Tenant destMember,
        ResolutionContext context)
    {
        return new Tenant
        {
            Id = source.TenantId,
            Name = source.Tenant?.Name ?? string.Empty,
        };
    }
}
