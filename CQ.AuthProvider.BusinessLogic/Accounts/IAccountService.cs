﻿using CQ.UnitOfWork.Abstractions.Repositories;

namespace CQ.AuthProvider.BusinessLogic.Accounts;

public interface IAccountService
{
    Task<CreateAccountResult> CreateAndSaveAsync(CreateAccountArgs args);

    Task<Account> CreateAndSaveAsync(
        CreateAccountForArgs args,
        AccountLogged accountLogged);
        
    Task<CreateAccountResult> CreateAndSaveWithTenantAsync(CreateAccountWithTenantArgs args);

    Task UpdatePasswordAsync(
        UpdatePasswordArgs args,
        AccountLogged accountLogged);

    Task<Pagination<Account>> GetAllAsync(
        int page,
        int pageSize,
        AccountLogged accountLogged);

    Task UpdateRolesAsync
        (Guid id,
        UpdateRolesArgs args,
        AccountLogged accountLogged);
}

internal interface IAccountInternalService
    : IAccountService
{
    Task<CreateAccountResult> CreateIdentityAndSaveAsync(
        Account account,
        string password,
        bool passwordIsHash = false);

    Task AssertByEmailAsync(string email);
}
