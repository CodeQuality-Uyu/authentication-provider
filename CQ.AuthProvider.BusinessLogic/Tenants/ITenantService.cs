﻿using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.UnitOfWork.Abstractions.Repositories;

namespace CQ.AuthProvider.BusinessLogic.Tenants;

public interface ITenantService
{
    Task CreateAsync(
        CreateTenantArgs args,
        AccountLogged accountLogged);

    Task<Pagination<Tenant>> GetAllAsync(
        int page = 1,
        int pageSize = 10);

    Task<Tenant> GetByIdAsync(
        string id,
        AccountLogged accountLogged);

    Task UpdateNameByIdAndSaveAsync(
        string id,
        string newName);

    Task UpdateOwnerAsync(
        string id,
        string newOwnerId,
        AccountLogged accountLogged);
}

internal interface ITenantInternalService
    : ITenantService
{
}


