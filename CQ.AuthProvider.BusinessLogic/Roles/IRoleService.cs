using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.UnitOfWork.Abstractions.Repositories;

namespace CQ.AuthProvider.BusinessLogic.Roles;

public interface IRoleService
{
    Task<Pagination<Role>> GetAllAsync(
        Guid? appId,
        bool? isPrivate,
        int page,
        int pageSize,
        AccountLogged accountLogged);

    Task CreateAsync(
        CreateRoleArgs args,
        AccountLogged accountLogged);

    Task CreateBulkAsync(
        CreateBulkRoleArgs args,
        AccountLogged accountLogged);

    Task AddPermissionByIdAsync(
        Guid id,
        AddPermissionArgs permissions);

    Task RemovePermissionByIdAsync(
        Guid id,
        Guid permissionId);

    Task AddAppsByIdAsync(
        Guid id,
        AddAppsArgs args,
        AccountLogged accountLogged);

    Task RemoveAppByIdAsync(
        Guid id,
        Guid appId,
        AccountLogged accountLogged);
}

internal interface IRoleInternalService
    : IRoleService
{
}

