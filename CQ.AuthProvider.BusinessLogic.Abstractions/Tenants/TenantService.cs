﻿using CQ.AuthProvider.BusinessLogic.Abstractions.Accounts;
using CQ.AuthProvider.BusinessLogic.Abstractions.Permissions;
using CQ.UnitOfWork.Abstractions.Repositories;
using System.IO.MemoryMappedFiles;

namespace CQ.AuthProvider.BusinessLogic.Abstractions.Tenants;

internal sealed class TenantService(
    ITenantRepository tenantRepository,
    IAccountRepository accountRepository)
    : ITenantInternalService
{
    public async Task CreateAsync(
        CreateTenantArgs args,
        AccountLogged accountLogged)
    {
        var existTenant = await tenantRepository
            .ExistByNameAsync(args.Name)
            .ConfigureAwait(false);

        if (existTenant)
        {
            throw new InvalidOperationException("Invalid name");
        }

        var tenant = new Tenant(
            args.Name,
            accountLogged);

        await tenantRepository
            .CreateAndSaveAsync(tenant)
            .ConfigureAwait(false);

        await accountRepository
            .UpdateByIdAsync(
            accountLogged.Id,
            tenant)
            .ConfigureAwait(false);
    }

    public async Task<Pagination<Tenant>> GetAllAsync(
        int page = 1,
        int pageSize = 10)
    {
        var tenants = await tenantRepository
            .GetPaginatedAsync(
            page,
            pageSize)
            .ConfigureAwait(false);

        return tenants;
    }

    public async Task<Tenant> GetByIdAsync(
        string id,
        AccountLogged accountLogged)
    {
        var tenant = await tenantRepository
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        if (tenant.Owner.Id != accountLogged.Id &&
            accountLogged.HasPermission(PermissionKey.FullAccess))
        {
            throw new InvalidOperationException("Invalid tenant");
        }

        return tenant;
    }
}