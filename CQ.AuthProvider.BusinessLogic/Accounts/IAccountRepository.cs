using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.UnitOfWork.Abstractions.Repositories;
namespace CQ.AuthProvider.BusinessLogic.Accounts;

public interface IAccountRepository
{
    Task CreateAsync(Account account);

    Task<bool> ExistByEmailAsync(string email);

    Task<Account> GetByEmailAsync(string email);

    Task<Account> GetByIdAsync(
        Guid id,
        Guid appId);

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
        Guid? appId,
        int page,
        int pageSize);

    Task AddAppAsync(
        App app,
        AccountLogged accountLogged);

    /// <summary>
    /// Tenant e ids de <b>todos</b> los roles de la cuenta, sin filtrar por app. <c>null</c> si la
    /// cuenta no existe.
    /// </summary>
    Task<(Guid TenantId, List<Guid> RoleIds)?> GetRolesSnapshotByIdAsync(Guid id);

    /// <summary>Quita roles de la cuenta <paramref name="accountId"/>.</summary>
    Task DeleteRolesByIdAsync(
        Guid accountId,
        List<Guid> rolesIds);

    /// <summary>Agrega roles a la cuenta <paramref name="accountId"/>.</summary>
    Task AddRolesByIdAsync(
        Guid accountId,
        List<Guid> rolesIds);

    Task DeleteAndSaveByIdAsync(Guid id);

    Task UpdateEmailVerifiedByIdAsync(Guid id);
}
