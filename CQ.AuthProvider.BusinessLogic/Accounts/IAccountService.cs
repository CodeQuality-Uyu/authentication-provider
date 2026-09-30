using CQ.UnitOfWork.Abstractions.Repositories;

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
        Guid? tenantId,
        Guid? appId,
        int page,
        int pageSize,
        AccountLogged accountLogged);

    Task<Account> GetByIdAsync(
        Guid id,
        AccountLogged accountLogged);

    Task UpdateRolesAsync
        (Guid id,
        UpdateRolesArgs args,
        AccountLogged accountLogged);

    Task DeleteFromAppAsync(AccountLogged accountLogged);

    /// <summary>
    /// Lo mismo que <see cref="DeleteFromAppAsync"/>, pero sobre otra cuenta: la saca de
    /// <paramref name="appId"/>, o del app con el que se logueó <paramref name="accountLogged"/>
    /// si no viene. Otra app pide alcance extra (ver <see cref="AccountLogged.AssertCanReachApp"/>).
    /// </summary>
    Task DeleteFromAppByIdAsync(
        Guid id,
        Guid? appId,
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
