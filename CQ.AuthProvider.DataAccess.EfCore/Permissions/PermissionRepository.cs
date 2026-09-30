using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Permissions;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.AuthProvider.DataAccess.EfCore.Apps;
using CQ.UnitOfWork.Abstractions.Repositories;
using CQ.UnitOfWork.EfCore.Core;
using CQ.UnitOfWork.EfCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.AuthProvider.DataAccess.EfCore.Permissions;

internal sealed class PermissionRepository(
    AuthDbContext context,
    [FromKeyedServices(MapperKeyedService.DataAccess)] IMapper mapper,
    IRepository<PermissionApp> permissionAppRepository)
    : EfCoreRepository<PermissionEfCore>(context),
    IPermissionRepository
{
    public async Task<IList<Permission>> GetAllAsync(IList<Guid> ids)
    {
        var permissions = await Entities
            .Where(p => ids.Contains(p.Id))
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);

        return mapper.Map<IList<Permission>>(permissions);
    }
    public async Task<Pagination<Permission>> GetAllAsync(
        Guid? appId,
        bool? isPrivate,
        Guid? roleId,
        string? search,
        int page,
        int pageSize,
        AccountLogged accountLogged)
    {
        var appLoggedIsAuthWebApi = accountLogged.AppLogged.Id == AuthConstants.AUTH_WEB_API_APP_ID;

        // Lowered here so the comparison is case insensitive on providers with a
        // case sensitive collation (Postgres).
        var searchFilter = string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToLower();

        var query = Entities
            .Include(p => p.App)
            .AsNoTracking()
            .Where(p => (appLoggedIsAuthWebApi && p.AppId == AuthConstants.AUTH_WEB_API_APP_ID) || p.TenantId == accountLogged.Tenant.Id)
            .Where(p => isPrivate == null || p.IsPublic == !isPrivate)
            .Where(p => roleId == null || p.Roles.Any(r => r.Id == roleId))
            // Un solo texto para nombre y key: un buscador no sabe cuál de los dos escribió
            // quien busca.
            .Where(p => searchFilter == null
                || p.Name.ToLower().Contains(searchFilter)
                || p.Key.ToLower().Contains(searchFilter));

        // Sin appId el alcance sigue siendo todo el tenant, como antes. Con appId, ahora son los
        // permisos *efectivos* para esa app y no solo los suyos: ver EffectiveScope.
        if (appId.HasValue)
        {
            query = query.EffectiveForApp(context, appId.Value);
        }

        var permissions = await query
            .ToPaginateAsync(page, pageSize)
            .ConfigureAwait(false);

        return mapper.Map<Pagination<Permission>>(permissions);
    }

    public async Task<List<Permission>> GetAllByKeysAsync(
        Guid appId,
        List<string> keys,
        AccountLogged accountLogged)
    {
        //var keyesMapped = JsonConvert.SerializeObject(keys);

        //// Define raw SQL query
        //var sql = @"
        //SELECT p.*
        //FROM Permissions AS p
        //INNER JOIN OPENJSON(@keyAppJson) 
        //WITH (
        //    Item1 UNIQUEIDENTIFIER,
        //    Item2 NVARCHAR(MAX)
        //) AS j
        //ON [p].[AppId] = [j].[Item1] AND [p].[Key] = [j].[Item2]
        //WHERE p.TenantId = @tenantId AND @keyAppJson IS NOT NULL AND LEN(@keyAppJson) > 2";

        //// Execute the raw query
        //var permissions = await Entities
        //    .FromSqlRaw(sql,
        //        new SqlParameter("@keyAppJson", keyesMapped),
        //        new SqlParameter("@tenantId", accountLogged.Tenant.Id))
        //    .ToListAsync()
        //    .ConfigureAwait(false);


        // Alcance efectivo y no solo p.AppId == appId: es lo que permite armar un rol en una app
        // hija usando permisos heredados del padre.
        var permissions = await Entities
            .Where(p => keys.Any(k => p.Key == k))
            .Where(p => p.TenantId == accountLogged.Tenant.Id)
            .EffectiveForApp(context, appId)
            .ToListAsync()
            .ConfigureAwait(false);

        return mapper.Map<List<Permission>>(permissions);
    }

    async Task IPermissionRepository.CreateBulkAndSaveAsync(List<Permission> permissions)
    {
        var permissionsEfCore = permissions.ConvertAll(p => new PermissionEfCore(p));

        await CreateBulkAndSaveAsync(permissionsEfCore).ConfigureAwait(false);
    }

    public async Task UpdateAndSaveByIdAsync(
        Guid id,
        UpdatePermissionArgs args,
        AccountLogged accountLogged)
    {
        await Entities
            .Where(p => p.Id == id)
            .Where(p => p.TenantId == accountLogged.Tenant.Id)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(p => p.Name, args.Name)
                .SetProperty(p => p.Description, args.Description)
                .SetProperty(p => p.Key, args.Key)
                .SetProperty(p => p.IsPublic, args.IsPublic)
                .SetProperty(p => p.AppId, args.AppId))
            .ConfigureAwait(false);
    }

    public async Task UpdateBulkAndSaveAsync(
        List<UpdatePermissionByIdArgs> permissions,
        AccountLogged accountLogged)
    {
        var tenantId = accountLogged.Tenant.Id;

        // Each permission can move to a different app / key, so they are updated
        // individually (per-row values can't be expressed in a single statement).
        // The whole batch runs in one transaction so it's all-or-nothing.
        await using var transaction = await context.Database
            .BeginTransactionAsync()
            .ConfigureAwait(false);

        foreach (var permission in permissions)
        {
            await Entities
                .Where(p => p.Id == permission.Id)
                .Where(p => p.TenantId == tenantId)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(p => p.Name, permission.Name)
                    .SetProperty(p => p.Description, permission.Description)
                    .SetProperty(p => p.Key, permission.Key)
                    .SetProperty(p => p.IsPublic, permission.IsPublic)
                    .SetProperty(p => p.AppId, permission.AppId))
                .ConfigureAwait(false);
        }

        await transaction
            .CommitAsync()
            .ConfigureAwait(false);
    }

    public async Task<(Guid AppId, Guid TenantId)?> GetOwnerByIdAsync(Guid id)
    {
        var owner = await Entities
            .Where(p => p.Id == id)
            .Select(p => new { p.AppId, p.TenantId })
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
            .PermissionsApps
            .Where(pa => pa.PermissionId == id)
            .Select(pa => pa.AppId)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task AddAppsAsync(
        Guid id,
        List<Guid> appIds)
    {
        var grants = appIds.ConvertAll(appId => new PermissionApp
        {
            PermissionId = id,
            AppId = appId,
        });

        await permissionAppRepository
            .CreateBulkAndSaveAsync(grants)
            .ConfigureAwait(false);
    }

    public async Task RemoveAppByIdAsync(
        Guid id,
        Guid appId)
    {
        await permissionAppRepository
            .DeleteAndSaveAsync(pa => pa.PermissionId == id && pa.AppId == appId)
            .ConfigureAwait(false);
    }
}
