using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Roles;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.AuthProvider.DataAccess.EfCore.Apps;
using CQ.AuthProvider.DataAccess.EfCore.Permissions;
using CQ.UnitOfWork.Abstractions.Repositories;
using CQ.UnitOfWork.EfCore.Core;
using CQ.UnitOfWork.EfCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.AuthProvider.DataAccess.EfCore.Roles;

internal sealed class RoleRepository(
    AuthDbContext context,
    [FromKeyedServices(MapperKeyedService.DataAccess)] IMapper mapper,
    IRepository<PermissionEfCore> permissionRepository,
    IRepository<RolePermission> rolePermissionRepository,
    IRepository<RoleApp> roleAppRepository)
    : EfCoreRepository<RoleEfCore>(context),
    IRoleRepository
{
    public async Task<Pagination<Role>> GetAllAsync(
        Guid appId,
        bool? isPrivate,
        int page,
        int pageSize,
        AccountLogged accountLogged)
    {

        var query = Entities
            .Include(r => r.App)
            .Where(r => r.TenantId == accountLogged.Tenant.Id)
            .Where(r => isPrivate == null || r.IsPublic == !isPrivate)
            .EffectiveForApp(context, appId)
            .AsNoTracking()
            .AsSplitQuery();

        var roles = await query
            .ToPaginateAsync(page, pageSize)
            .ConfigureAwait(false);

        return mapper.Map<Pagination<Role>>(roles);
    }

    public async Task RemoveDefaultsAsync(
        Guid appId,
        AccountLogged accountLogged)
    {
        var query = Entities
            .Where(r => r.TenantId == accountLogged.Tenant.Id)
            .Where(r => r.AppId == appId)
            .Where(r => r.IsDefault);

        var roles = await query
            .ToListAsync()
            .ConfigureAwait(false);

        roles.ForEach(a =>
        {
            a.IsDefault = false;
        });

        Entities.UpdateRange(roles);
    }

    /*
     * Include used in: RoleService
     */
    public new async Task<Role> GetByIdAsync(Guid id)
    {
        var query = Entities
           .Include(r => r.Permissions)
           .Where(r => r.Id == id);

        var role = await query
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        AssertNullEntity(role, id, nameof(Role.Id));

        return mapper.Map<Role>(role);
    }

    public async Task<Role> GetDefaultByTenantIdAsync(
        Guid? roleId,
        Guid appId,
        Guid tenantId)
    {
        var query = Entities
            // TODO check in r.AppId and inheritence app
            //.Where(r => r.AppId == appId)
            .Where(r => r.TenantId == tenantId)
            .Where(r => (!roleId.HasValue && r.IsDefault) || r.Id == roleId);

        var role = await query
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        AssertNullEntity(role, tenantId, nameof(Role.Tenant));

        return mapper.Map<Role>(role);
    }

    public async Task CreateBulkAsync(List<Role> roles)
    {
        var permissions = roles
        .SelectMany(r => r.Permissions.ConvertAll(p => new RolePermission
        {
            RoleId = r.Id,
            PermissionId = p.Id
        }));

        await BaseContext
        .AddRangeAsync(permissions)
        .ConfigureAwait(false);

        var rolesEfCore = roles.ConvertAll(r => new RoleEfCore(r));

        await Entities.AddRangeAsync(rolesEfCore).ConfigureAwait(false);
    }

    public async Task AddPermissionsAsync(
        Guid id,
        List<Guid> permissionIds)
    {
        var rolePermissions = permissionIds
        .ConvertAll(p => new RolePermission
        {
            RoleId = id,
            PermissionId = p
        });

        await rolePermissionRepository
            .CreateBulkAndSaveAsync(rolePermissions)
            .ConfigureAwait(false);
    }

    public async Task RemovePermissionByIdAsync(
        Guid id,
        Guid permissionId)
    {
        await rolePermissionRepository
            .DeleteAndSaveAsync(rp => rp.RoleId == id && rp.PermissionId == permissionId)
            .ConfigureAwait(false);
    }

    public async Task<Role?> GetDefaultOrDefaultByAppIdAndTenantIdAsync(
        Guid appId,
        Guid tenantId)
    {
        var query = Entities
            .Where(r => r.AppId == appId)
            .Where(r => r.TenantId == tenantId)
            .Where(r => r.IsDefault);

        var entity = await query
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        return mapper.Map<Role>(entity);
    }

    public async Task<List<Role>> GetAllByIdsAsync(
        List<Guid> ids,
        AccountLogged accountLogged)
    {

        var query = Entities
            .Where(r => r.TenantId == accountLogged.Tenant.Id)
            .EffectiveForAnyApp(context, accountLogged.AppsIds)
            .Where(r => ids.Any(i => i == r.Id));

        var roles = await query
            .ToListAsync()
            .ConfigureAwait(false);

        return mapper.Map<List<Role>>(roles);
    }

    public async Task<List<Role>> GetByIdAsync(
        List<Guid> ids,
        List<Guid> appIds,
        Guid tenantId)
    {
        List<Guid> authRoles = [AuthConstants.CLIENT_OWNER_ROLE_ID, AuthConstants.APP_OWNER_ROLE_ID, AuthConstants.TENANT_OWNER_ROLE_ID];

        var query = Entities
            .Where(r => ids.Contains(r.Id) && (authRoles.Contains(r.Id) || r.TenantId == tenantId))
            // TODO check in r.AppId and inheritence app
            //.Where(r => appIds.Contains(r.AppId))
            ;

        var roles = await query
            .ToListAsync()
            .ConfigureAwait(false);

        if (roles.Count != ids.Count)
        {
            var foundIds = roles.Select(r => r.Id).ToList();
            var notFoundIds = ids.Except(foundIds);
            throw new KeyNotFoundException($"Roles not found for Ids: {string.Join(", ", notFoundIds)}");
        }

        return mapper.Map<List<Role>>(roles);
    }

    public async Task DeleteAndSaveByIdAsync(Guid id)
    {
        await DeleteAndSaveAsync(r => r.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<List<string>> GetAllByAppAndNamesAsync(
    Guid appId,
    List<string> roles)
    {
        var entities = await Entities
            .AsNoTracking()
            .Where(r => r.AppId == appId)
            .Where(r => roles.Contains(r.Name))
            .Select(r => r.Name)
            .ToListAsync()
            .ConfigureAwait(false);

        return entities;
    }

    public async Task<(Guid AppId, Guid TenantId)?> GetOwnerByIdAsync(Guid id)
    {
        var owner = await Entities
            .Where(r => r.Id == id)
            .Select(r => new { r.AppId, r.TenantId })
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        return owner is null
            ? null
            : (owner.AppId, owner.TenantId);
    }

    public async Task<List<Guid>> GetGrantedAppIdsAsync(Guid id)
    {
        return await context
            .RolesApps
            .Where(ra => ra.RoleId == id)
            .Select(ra => ra.AppId)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task AddAppsAsync(
        Guid id,
        List<Guid> appIds)
    {
        var grants = appIds.ConvertAll(appId => new RoleApp
        {
            RoleId = id,
            AppId = appId,
        });

        await roleAppRepository
            .CreateBulkAndSaveAsync(grants)
            .ConfigureAwait(false);
    }

    public async Task RemoveAppByIdAsync(
        Guid id,
        Guid appId)
    {
        await roleAppRepository
            .DeleteAndSaveAsync(ra => ra.RoleId == id && ra.AppId == appId)
            .ConfigureAwait(false);
    }
}
