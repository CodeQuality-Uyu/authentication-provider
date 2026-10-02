using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.UnitOfWork.Abstractions.Repositories;

namespace CQ.AuthProvider.BusinessLogic.Permissions;

public interface IPermissionService
{
    Task<Pagination<Permission>> GetAllAsync(
        Guid? appId,
        bool? isPrivate,
        Guid? roleId,
        string? search,
        int page,
        int pageSize,
        AccountLogged accountLogged);

    Task<Permission> CreateAsync(
        CreatePermissionArgs args,
        AccountLogged accountLogged);

    Task<List<Permission>> CreateBulkAsync(
        CreateBulkPermissionArgs args,
        AccountLogged accountLogged);

    Task UpdateAsync(
        Guid id,
        UpdatePermissionArgs args,
        AccountLogged accountLogged);

    Task UpdateBulkAsync(
        UpdateBulkPermissionArgs args,
        AccountLogged accountLogged);

    Task AddAppsByIdAsync(
        Guid id,
        AddAppsArgs args,
        AccountLogged accountLogged);

    Task RemoveAppByIdAsync(
        Guid id,
        Guid appId,
        AccountLogged accountLogged);
}

internal interface IPermissionInternalService
    : IPermissionService
{
}