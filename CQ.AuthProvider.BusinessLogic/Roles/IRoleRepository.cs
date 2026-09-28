using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.UnitOfWork.Abstractions.Repositories;

namespace CQ.AuthProvider.BusinessLogic.Roles;

public interface IRoleRepository
{
    Task<Pagination<Role>> GetAllAsync(
        Guid appId,
        bool? isPrivate,
        int page,
        int pageSize,
        AccountLogged accountLogged);

    Task RemoveDefaultsAsync(
        Guid appId,
        AccountLogged accountLogged);

    Task CreateBulkAsync(List<Role> roles);

    Task<Role> GetByIdAsync(Guid id);

    Task AddPermissionsAsync(
        Guid id,
        List<Guid> permissionIds);

    Task RemovePermissionByIdAsync(
        Guid id,
        Guid permissionId);

    Task<Role> GetDefaultByTenantIdAsync(
        Guid? roleId,
        Guid appId,
        Guid tenantId);

    Task<Role?> GetDefaultOrDefaultByAppIdAndTenantIdAsync(
        Guid appId,
        Guid tenantId);

    Task<List<Role>> GetByIdAsync(
        List<Guid> ids,
        List<Guid> appIds,
        Guid tenantId);

    Task<List<Role>> GetAllByIdsAsync(
        List<Guid> ids,
        AccountLogged accountLogged);

    Task DeleteAndSaveByIdAsync(Guid id);

    Task<List<string>> GetAllByAppAndNamesAsync(Guid appId, List<string> roles);

    /// <summary>App dueña y tenant del rol, o <c>null</c> si el rol no existe.</summary>
    Task<(Guid AppId, Guid TenantId)?> GetOwnerByIdAsync(Guid id);

    /// <summary>Apps que tienen un grant explícito de este rol.</summary>
    Task<List<Guid>> GetGrantedAppIdsAsync(Guid id);

    Task AddAppsAsync(
        Guid id,
        List<Guid> appIds);

    Task RemoveAppByIdAsync(
        Guid id,
        Guid appId);
}
