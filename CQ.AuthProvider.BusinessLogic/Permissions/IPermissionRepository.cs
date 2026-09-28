using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.UnitOfWork.Abstractions.Repositories;

namespace CQ.AuthProvider.BusinessLogic.Permissions;

public interface IPermissionRepository
{
    Task<Pagination<Permission>> GetAllAsync(
        Guid? appId,
        bool? isPrivate,
        Guid? roleId,
        string? name,
        string? key,
        int page,
        int pageSize,
        AccountLogged accountLogged);

    Task<IList<Permission>> GetAllAsync(IList<Guid> ids);

    Task<List<Permission>> GetAllByKeysAsync(
        Guid appId,
        List<string> keys,
        AccountLogged accountLogged);

    Task CreateBulkAndSaveAsync(List<Permission> permissions);

    Task UpdateAndSaveByIdAsync(
        Guid id,
        UpdatePermissionArgs args,
        AccountLogged accountLogged);

    Task UpdateBulkAndSaveAsync(
        List<UpdatePermissionByIdArgs> permissions,
        AccountLogged accountLogged);

    /// <summary>App dueña y tenant del permiso, o <c>null</c> si el permiso no existe.</summary>
    Task<(Guid AppId, Guid TenantId)?> GetOwnerByIdAsync(Guid id);

    /// <summary>Apps que tienen un grant explícito de este permiso.</summary>
    Task<List<Guid>> GetGrantedAppIdsAsync(Guid id);

    Task AddAppsAsync(
        Guid id,
        List<Guid> appIds);

    Task RemoveAppByIdAsync(
        Guid id,
        Guid appId);
}
