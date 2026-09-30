using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.UnitOfWork.Abstractions.Repositories;
namespace CQ.AuthProvider.BusinessLogic.Accounts;

public interface IAccountRepository
{
    Task CreateAsync(Account account);

    Task<bool> ExistByEmailAsync(string email);

    Task<Account> GetByEmailAsync(string email);

    /// <param name="tenantId">
    /// Si viene, una cuenta de otro tenant se trata como inexistente. <c>null</c> no filtra.
    /// </param>
    Task<Account> GetByIdAsync(
        Guid id,
        Guid appId,
        Guid? tenantId = null);

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

    /// <param name="tenantId"><c>null</c> trae las cuentas de todos los tenants.</param>
    /// <remarks>Cada cuenta viene con todos sus roles, sus apps y su tenant.</remarks>
    Task<Pagination<Account>> GetAllAsync(
        Guid? tenantId,
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

    Task RemoveAppAndSaveByIdAsync(
        Guid id,
        Guid appId);

    Task UpdateEmailVerifiedByIdAsync(Guid id);
}
