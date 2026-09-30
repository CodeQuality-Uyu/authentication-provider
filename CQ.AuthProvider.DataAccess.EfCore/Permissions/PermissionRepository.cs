using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Permissions;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.UnitOfWork.Abstractions.Repositories;
using CQ.UnitOfWork.EfCore.Core;
using CQ.UnitOfWork.EfCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.AuthProvider.DataAccess.EfCore.Permissions;

internal sealed class PermissionRepository(
    AuthDbContext context,
    [FromKeyedServices(MapperKeyedService.DataAccess)] IMapper mapper)
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
        string? name,
        string? key,
        int page,
        int pageSize,
        AccountLogged accountLogged)
    {
        var appLoggedIsAuthWebApi = accountLogged.AppLogged.Id == AuthConstants.AUTH_WEB_API_APP_ID;

        // Lowered here so the comparison is case insensitive on providers with a
        // case sensitive collation (Postgres).
        var nameFilter = string.IsNullOrWhiteSpace(name) ? null : name.Trim().ToLower();
        var keyFilter = string.IsNullOrWhiteSpace(key) ? null : key.Trim().ToLower();

        var query = Entities
            .Include(p => p.App)
            .AsNoTracking()
            .Where(p => (appLoggedIsAuthWebApi && p.AppId == AuthConstants.AUTH_WEB_API_APP_ID) || p.TenantId == accountLogged.Tenant.Id)
            .Where(p => isPrivate == null || p.IsPublic == !isPrivate)
            .Where(p => roleId == null || p.Roles.Any(r => r.Id == roleId))
            .Where(p => appId == null || p.AppId == appId)
            .Where(p => nameFilter == null || p.Name.ToLower().Contains(nameFilter))
            .Where(p => keyFilter == null || p.Key.ToLower().Contains(keyFilter));

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


        var permissions = await Entities
            .Where(p => keys.Any(k => p.Key == k))
            .Where(p => p.AppId == appId)
            .Where(p => p.TenantId == accountLogged.Tenant.Id)
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
}
