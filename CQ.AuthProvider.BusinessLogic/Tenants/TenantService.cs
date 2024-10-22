﻿using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Roles;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.UnitOfWork.Abstractions;
using CQ.UnitOfWork.Abstractions.Repositories;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.Tenants;

internal sealed class TenantService(
    ITenantRepository _tenantRepository,
    IAccountRepository _accountRepository,
    IRoleRepository _roleRepository,
    IUnitOfWork _unitOfWork)
    : ITenantInternalService
{
    public async Task CreateAsync(
        CreateTenantArgs args,
        AccountLogged accountLogged)
    {
        await AssertNameAsync(args.Name)
            .ConfigureAwait(false);

        var tenant = new Tenant(
            args.Name,
            accountLogged);

        await _tenantRepository
            .CreateAndSaveAsync(tenant)
            .ConfigureAwait(false);

        await _accountRepository
            .UpdateTenantByIdAndSaveAsync(
            accountLogged.Id,
            tenant)
            .ConfigureAwait(false);
    }

    private async Task AssertNameAsync(string name)
    {
        var existTenant = await _tenantRepository
                    .ExistByNameAsync(name)
                    .ConfigureAwait(false);
        if (existTenant)
        {
            throw new InvalidOperationException("Name is in use");
        }
    }

    public async Task<Pagination<Tenant>> GetAllAsync(
        int page = 1,
        int pageSize = 10)
    {
        var tenants = await _tenantRepository
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
        var tenant = await _tenantRepository
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        return tenant;
    }

    public async Task UpdateNameByIdAndSaveAsync(
        string id,
        string newName)
    {
        await AssertNameAsync(newName)
            .ConfigureAwait(false);

        await _tenantRepository
            .UpdateNameByIdAndSaveAsync(id, newName)
            .ConfigureAwait(false);
    }


    public async Task UpdateOwnerAsync(
        string id,
        string newOwnerId,
        AccountLogged accountLogged)
    {
        var newOwner = await _accountRepository
            .GetByIdAsync(
            newOwnerId,
            nameof(Account.Tenant),
            nameof(Account.Roles))
            .ConfigureAwait(false);

        if (newOwner.TenantValue.Id != id)
        {
            throw new InvalidOperationException("New owner is not in tenant");
        }

        var isTenantOwner = newOwner.HasPermission(AuthConstants.TENANT_OWNER_ROLE_ID);
        if (isTenantOwner)
        {
            return;
        }

        await _tenantRepository
            .UpdateOwnerByIdAsync(
            id,
            newOwner);

        await _accountRepository
            .AddRoleByIdAsync(newOwnerId, AuthConstants.TENANT_OWNER_ROLE_ID)
            .ConfigureAwait(false);

        var newRole = await _roleRepository
            .GetDefaultOrDefaultByAppIdAndTenantIdAsync(AuthConstants.AUTH_WEB_API_APP_ID, id)
            .ConfigureAwait(false);
        if (Guard.IsNotNull(newRole))
        {
            await _accountRepository
                .AddRoleByIdAsync(accountLogged.Id, newRole.Id)
                .ConfigureAwait(false);
        }

        await _accountRepository
            .RemoveRoleByIdAsync(accountLogged.Id, AuthConstants.TENANT_OWNER_ROLE_ID)
            .ConfigureAwait(false);

        await _unitOfWork
            .CommitChangesAsync()
            .ConfigureAwait(false);
    }
}
