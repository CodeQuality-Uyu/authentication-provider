﻿using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Sessions;
using CQ.AuthProvider.BusinessLogic.Tenants;

namespace CQ.AuthProvider.DataAccess.EfCore.Sessions;

internal sealed class SessionProfile
    : Profile
{
    public SessionProfile()
    {
        CreateMap<SessionEfCore, Session>()
            .ConvertUsing((source, destination, options) => new Session
            {
                Id = source.Id,
                Token = source.Token,
                Account = options.Mapper.Map<Account>(source.Account),
                App = new App
                {
                    Id = source.AppId,
                    Name = source.App?.Name ?? string.Empty,
                    Tenant = new Tenant
                    {
                        Id = source.Account.TenantId,
                    }
                },
            });
    }
}
