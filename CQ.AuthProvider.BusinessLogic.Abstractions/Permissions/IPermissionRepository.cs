﻿
using CQ.AuthProvider.BusinessLogic.Abstractions.Accounts;

namespace CQ.AuthProvider.BusinessLogic.Abstractions.Permissions;

internal interface IPermissionRepository
{
    Task<List<Permission>> GetAllAsync(
        bool? isPrivate,
        string? roleId,
        Account accountLogged);

    Task<List<Permission>> GetAllByPermissionKeyAsync(List<PermissionKey> permisionsKeys);

    Task<bool> ExistByKeyAsync(PermissionKey permissionKey);

    Task CreateAsync(Permission permission);

    Task CreateBulkAsync(List<Permission> permissions);
}