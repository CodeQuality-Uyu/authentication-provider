using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Permissions;
using CQ.UnitOfWork.Abstractions;
using CQ.UnitOfWork.Abstractions.Repositories;
using System.Data;

namespace CQ.AuthProvider.BusinessLogic.Roles;

internal sealed class RoleService(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IAppRepository appRepository,
    IUnitOfWork unitOfWork)
    : IRoleInternalService
{
    public async Task<Pagination<Role>> GetAllAsync(
        Guid? appId,
        bool? isPrivate,
        int page,
        int pageSize,
        AccountLogged accountLogged)
    {
        var accountNotInApp = !accountLogged.AppsIds.Contains(appId ?? Guid.Empty);
        if (accountNotInApp)
        {
            appId = accountLogged.AppLogged.Id;
        }

        var roles = await roleRepository
            .GetAllAsync(
            appId!.Value,
            isPrivate,
            page,
            pageSize,
            accountLogged)
            .ConfigureAwait(false);

        return roles;
    }

    public async Task CreateAsync(
        CreateRoleArgs args,
        AccountLogged accountLogged)
    {
        await CreateBulkAsync(
            new CreateBulkRoleArgs([args], args.AppId),
            accountLogged)
            .ConfigureAwait(false);
    }

    public async Task CreateBulkAsync(
        CreateBulkRoleArgs args,
        AccountLogged accountLogged)
    {
        await AssertAsync(args)
            .ConfigureAwait(false);

        var defaultRoles = args
            .Roles
            .Any(r => r.IsDefault);

        if (defaultRoles)
        {
            await roleRepository
                .RemoveDefaultsAsync(
                args.AppId,
                accountLogged)
                .ConfigureAwait(false);
        }

        var allPermissionsKeyes = args
            .Roles
            .SelectMany(r => r.PermissionKeys)
            .Distinct()
            .ToList();

        var permissions = await permissionRepository
            .GetAllByKeysAsync(
                args.AppId,
                allPermissionsKeyes,
                accountLogged)
            .ConfigureAwait(false);
        if (permissions.Count != allPermissionsKeyes.Count)
        {
            var missingPermissions = allPermissionsKeyes
                .Where(pk => !permissions.Exists(p => p.Key == pk))
                .ToList();

            throw new InvalidOperationException($"The following permissions do not exist: {string.Join(",", missingPermissions)}");
        }

        var app = accountLogged.Apps.First(a => a.Id == args.AppId);
        var roles = args
            .Roles
            .ConvertAll(r =>
            {
                var permissionsOfRole = permissions
                    .Where(p => r.PermissionKeys.Contains(p.Key))
                    .ToList();

                return new Role
                {
                    Name = r.Name,
                    Description = r.Description,
                    IsPublic = r.IsPublic,
                    Permissions = permissionsOfRole,
                    IsDefault = r.IsDefault,
                    App = app,
                    Tenant = accountLogged.Tenant
                };
            });

        await roleRepository
            .CreateBulkAsync(roles)
            .ConfigureAwait(false);

        await unitOfWork
            .CommitChangesAsync()
            .ConfigureAwait(false);
    }

    private async Task AssertAsync(
        CreateBulkRoleArgs args)
    {
        var roles = args
        .Roles
        .Select(r => r.Name)
        .ToList();

        var rolesCreated = await roleRepository
            .GetAllByAppAndNamesAsync(args.AppId, roles)
            .ConfigureAwait(false);

        if (rolesCreated.Count != 0)
        {
            throw new InvalidOperationException($"The following roles already exist: {string.Join(",", rolesCreated)}");
        }
    }

    public async Task AddPermissionByIdAsync(
        Guid id,
        AddPermissionArgs args)
    {
        await AssertAsync(
            id,
            args)
            .ConfigureAwait(false);

        await roleRepository
            .AddPermissionsAsync(
            id,
            args.PermissionIds)
            .ConfigureAwait(false);
    }

    private async Task AssertAsync(
        Guid id,
        AddPermissionArgs args)
    {
        var role = await roleRepository
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        var duplicatedPermissions = role
            .Permissions
            .Where(p => args.PermissionIds.Contains(p.Id))
            .Select(p => p.Key)
            .ToList();
        if (duplicatedPermissions.Count != 0)
        {
            throw new InvalidOperationException($"The following permissions are duplicated in the role: {string.Join(",", duplicatedPermissions)}");
        }

        var permissions = await permissionRepository
            .GetAllAsync(args.PermissionIds)
            .ConfigureAwait(false);
        if (permissions.Count != args.PermissionIds.Count)
        {
            var foundIds = permissions.Select(p => p.Id).ToHashSet();
            var notFoundIds = args.PermissionIds.Where(pid => !foundIds.Contains(pid)).ToList();
            throw new InvalidOperationException($"The following permissions do not exist: {string.Join(",", notFoundIds)}");
        }
    }

    public async Task RemovePermissionByIdAsync(
        Guid id,
        Guid permissionId)
    {
        await roleRepository
            .RemovePermissionByIdAsync(
            id,
            permissionId)
            .ConfigureAwait(false);
    }

    public async Task AddAppsByIdAsync(
        Guid id,
        AddAppsArgs args,
        AccountLogged accountLogged)
    {
        var owner = await AssertOwnerInTenantAsync(id, accountLogged)
            .ConfigureAwait(false);

        await AssertAppsCanBeGrantedAsync(
            args.AppIds,
            owner.AppId,
            accountLogged)
            .ConfigureAwait(false);

        var alreadyGranted = await roleRepository
            .GetGrantedAppIdsAsync(id)
            .ConfigureAwait(false);

        var duplicated = args
            .AppIds
            .Intersect(alreadyGranted)
            .ToList();
        if (duplicated.Count != 0)
        {
            throw new InvalidOperationException($"The role is already granted to the following apps: {string.Join(",", duplicated)}");
        }

        await roleRepository
            .AddAppsAsync(id, args.AppIds)
            .ConfigureAwait(false);
    }

    public async Task RemoveAppByIdAsync(
        Guid id,
        Guid appId,
        AccountLogged accountLogged)
    {
        var owner = await AssertOwnerInTenantAsync(id, accountLogged)
            .ConfigureAwait(false);

        // Quitar el grant de la app dueña no tiene sentido: su alcance no viene de un grant, viene
        // de ser la dueña. Se avisa en vez de responder 2xx sin haber hecho nada.
        if (appId == owner.AppId)
        {
            throw new InvalidOperationException("The app that owns the role cannot be removed from its scope");
        }

        await roleRepository
            .RemoveAppByIdAsync(id, appId)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Verifica que el role exista y sea del tenant de la cuenta logueada, y devuelve su app
    /// dueña y su tenant.
    /// </summary>
    private async Task<(Guid AppId, Guid TenantId)> AssertOwnerInTenantAsync(
        Guid id,
        AccountLogged accountLogged)
    {
        var owner = await roleRepository
            .GetOwnerByIdAsync(id)
            .ConfigureAwait(false);

        if (!owner.HasValue)
        {
            throw new InvalidOperationException($"The role ({id}) does not exist");
        }

        if (owner.Value.TenantId != accountLogged.Tenant.Id)
        {
            throw new InvalidOperationException($"The role ({id}) does not belong to the tenant");
        }

        return owner.Value;
    }

    /// <summary>
    /// Un grant solo puede apuntar a una app <b>descendiente</b> de la app dueña, y del mismo
    /// tenant.
    /// </summary>
    /// <remarks>
    /// La restricción a descendientes es lo que mantiene la herencia en una sola dirección: un
    /// padre comparte hacia abajo, y ninguna app puede quedar con alcance sobre algo de una rama
    /// que no es la suya. El chequeo de tenant es redundante con el de descendencia (el árbol es
    /// intra-tenant), pero se hace igual y primero, porque da un error claro en el caso común de
    /// mandar un appId de otro tenant y porque no depende de que la tabla de cierre esté al día.
    /// </remarks>
    private async Task AssertAppsCanBeGrantedAsync(
        List<Guid> appIds,
        Guid ownerAppId,
        AccountLogged accountLogged)
    {
        if (appIds.Contains(ownerAppId))
        {
            throw new InvalidOperationException("The app that owns it is already in scope, it does not need a grant");
        }

        var inTenant = await appRepository
            .GetExistingIdsInTenantAsync(appIds, accountLogged.Tenant.Id)
            .ConfigureAwait(false);

        var outsideTenant = appIds
            .Except(inTenant)
            .ToList();
        if (outsideTenant.Count != 0)
        {
            throw new InvalidOperationException($"The following apps don't belong to the tenant: {string.Join(",", outsideTenant)}");
        }

        var descendants = await appRepository
            .GetDescendantIdsAsync(appIds, ownerAppId)
            .ConfigureAwait(false);

        var notDescendants = appIds
            .Except(descendants)
            .ToList();
        if (notDescendants.Count != 0)
        {
            throw new InvalidOperationException($"The following apps are not descendants of the app that owns it: {string.Join(",", notDescendants)}");
        }
    }
}
