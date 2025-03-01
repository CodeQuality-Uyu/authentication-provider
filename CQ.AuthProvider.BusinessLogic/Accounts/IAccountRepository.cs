﻿using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.UnitOfWork.Abstractions.Repositories;
namespace CQ.AuthProvider.BusinessLogic.Accounts;

public interface IAccountRepository
{
    Task CreateAsync(Account account);

    Task<bool> ExistByEmailAsync(string email);

    Task<Account> GetByEmailAsync(string email);

    Task<Account> GetByIdAsync(Guid id);

    Task<Account> GetByIdAsync(
        Guid id,
        AccountLogged accountLogged);

    Task<Account> GetByIdAsync(
        Guid id,
        params string[] includes);

    Task UpdateTenantByIdAsync(
        Guid id,
        Tenant tenant);

    Task AddRoleByIdAsync(
        Guid id,
        Guid roleId);

    Task RemoveRoleByIdAsync(
        Guid id,
        Guid roleId);

    Task<Pagination<Account>> GetAllAsync(
        Guid tenantId,
        int page,
        int pageSize);

    Task AddAppAsync(App app, AccountLogged accountLogged);

    Task DeleteRolesByIdAsync(
        List<Guid> rolesIds,
        Account account);

    Task AddRolesByIdAsync(
        List<Guid> rolesIds,
        Account account);

    Task DeleteAndSaveByIdAsync(Guid id);
}
