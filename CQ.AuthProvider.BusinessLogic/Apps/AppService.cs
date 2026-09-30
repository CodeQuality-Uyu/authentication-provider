using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Subscriptions;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.Blobs;
using CQ.UnitOfWork.Abstractions;
using CQ.UnitOfWork.Abstractions.Repositories;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.Apps;

internal sealed class AppService(
    IAppRepository appRepository,
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IBlobService blobService,
    ISubscriptionRepository subscriptionRepository)
    : IAppInternalService
{
    public async Task<App> CreateAsync(
        CreateAppArgs args,
        AccountLogged accountLogged)
    {
        var existAppWithName = await appRepository
            .ExistsByNameInTenantAsync(args.Name, accountLogged.Tenant.Id);
        if (existAppWithName)
        {
            throw new InvalidOperationException("Name is in used");
        }

        var promotedKeys = await PromoteLogoAsync(
            args.Logo,
            args.Name,
            accountLogged)
            .ConfigureAwait(false);

        try
        {
            var app = new App(
                args.Name,
                args.IsDefault,
                args.Logo,
                accountLogged.Tenant,
                null,
                args.AccountDataSource,
                args.GoogleClientId,
                args.RequiresEmailVerification);

            if (app.IsDefault)
            {
                var defaultApp = await appRepository
                    .GetOrDefaultByDefaultAsync(app.Tenant.Id)
                    .ConfigureAwait(false);
                if (Guard.IsNotNull(defaultApp))
                {
                    await appRepository
                        .RemoveDefaultByIdAsync(defaultApp.Id)
                        .ConfigureAwait(false);
                }
            }

            await appRepository
                .CreateAsync(app)
                .ConfigureAwait(false);

            if (args.RegisterToIt)
            {
                await accountRepository
                   .AddAppAsync(app, accountLogged)
                   .ConfigureAwait(false);
            }

            var subscription = await subscriptionRepository
                .CreateAsync(app)
                .ConfigureAwait(false);

            await unitOfWork
                .CommitChangesAsync()
                .ConfigureAwait(false);

            return app;
        }
        catch
        {
            await RollbackBlobsAsync(promotedKeys).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<App> CreateClientAsync(
        CreateClientAppArgs args,
        AccountLogged accountLogged)
    {
        var existAppWithName = await appRepository
            .ExistsByNameInTenantAsync(args.Name, accountLogged.Tenant.Id);
        if (existAppWithName)
        {
            throw new InvalidOperationException("Name is in used");
        }

        var promotedKeys = await PromoteLogoAsync(
            args.Logo,
            args.Name,
            accountLogged)
            .ConfigureAwait(false);

        try
        {
            var app = new App(
                args.Name,
                false,
                args.Logo ?? accountLogged.AppLogged.Logo,
                accountLogged.Tenant,
                accountLogged.AppLogged,
                args.AccountDataSource,
                args.GoogleClientId,
                args.RequiresEmailVerification);

            await appRepository
                .CreateAsync(app)
                .ConfigureAwait(false);

            var subscription = await subscriptionRepository
                .CreateAsync(app)
                .ConfigureAwait(false);

            await unitOfWork
                .CommitChangesAsync()
                .ConfigureAwait(false);

            return app;
        }
        catch
        {
            await RollbackBlobsAsync(promotedKeys).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Promueve los temporales del logo a <c>{tenant}/{app}/</c>, el formato de las keys que ya
    /// existen, y deja las keys definitivas en <paramref name="logo"/>.
    /// </summary>
    /// <remarks>
    /// Sólo se aceptan temporales del tenant de la cuenta: sin eso, cualquiera podría usar la
    /// subida de otro tenant.
    /// </remarks>
    /// <returns>Las keys promovidas, para borrarlas si el alta falla.</returns>
    private async Task<List<string>> PromoteLogoAsync(
        Logo? logo,
        string appName,
        AccountLogged accountLogged)
    {
        var tenantName = accountLogged.Tenant.Name;
        var folder = BlobKey.Combine(BlobKey.Slug(tenantName), BlobKey.Slug(appName));
        var promoted = new List<string>();

        async Task<string> PromoteAsync(string temporaryKey)
        {
            var key = await blobService
                .PromoteAsync(temporaryKey, folder, tenantName)
                .ConfigureAwait(false);
            promoted.Add(key);

            return key;
        }

        // Una app cliente sin logo propio usa el de su app padre: no hay nada que promover.
        if (logo is null)
        {
            return promoted;
        }

        try
        {
            logo.ColorKey = await PromoteAsync(logo.ColorKey).ConfigureAwait(false);
            logo.LightKey = await PromoteAsync(logo.LightKey).ConfigureAwait(false);
            logo.DarkKey = await PromoteAsync(logo.DarkKey).ConfigureAwait(false);
        }
        catch
        {
            // Si falla uno del medio, los anteriores ya se copiaron.
            await RollbackBlobsAsync(promoted).ConfigureAwait(false);
            throw;
        }

        return promoted;
    }

    /// <summary>
    /// Borra lo promovido para un alta que no se guardó. No propaga errores de borrado, para
    /// no tapar el del alta.
    /// </summary>
    private Task RollbackBlobsAsync(List<string> promotedKeys)
        => Task.WhenAll(promotedKeys.Select(key => blobService.RollbackAsync(new BlobReplacement(key, null))));

    public async Task<App> GetByIdAsync(Guid id)
    {
        var app = await appRepository
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        return app;
    }

    public async Task<List<App>> GetByIdAsync(List<Guid> ids)
    {
        var apps = await appRepository
            .GetByIdAsync(ids)
            .ConfigureAwait(false);

        return apps;
    }

    public async Task<Pagination<App>> GetPaginationAsync(
        Guid? fatherAppId,
        int page,
        int pageSize,
        AccountLogged accountLogged)
    {
        var hasListAppsPermission = accountLogged.HasPermission("getall-app");
        var hasListOwnClientsPermission = accountLogged.HasPermission("getall-client");

        if (!hasListAppsPermission && hasListOwnClientsPermission)
        {
            fatherAppId = accountLogged.AppLogged.Id;
        }

        var apps = await appRepository
            .GetPaginationAsync(
            accountLogged.Tenant.Id,
            fatherAppId,
            page,
            pageSize)
            .ConfigureAwait(false);

        return apps;
    }

    public async Task UpdateByIdAsync(
        Guid id,
        UpdateAppArgs args,
        AccountLogged accountLogged)
    {
        var hasApp = accountLogged.AppsIds.Contains(id);
        var isWebApiOwner = accountLogged.IsInRole(AuthConstants.AUTH_WEB_API_OWNER_ROLE_ID);
        var isTenantOwner = accountLogged.IsInRole(AuthConstants.TENANT_OWNER_ROLE_ID);

        if (!hasApp && !isWebApiOwner && !isTenantOwner)
        {
            throw new InvalidOperationException("Account doesn't belong to app");
        }

        await appRepository
            .UpdateAndSaveByIdAsync(
                id,
                args.Name,
                args.AccountDataSource,
                args.GoogleClientId,
                args.RequiresEmailVerification)
            .ConfigureAwait(false);
    }

    public async Task UpdateFatherByIdAsync(
        Guid id,
        UpdateAppFatherArgs args,
        AccountLogged accountLogged)
    {
        var tenantId = accountLogged.Tenant.Id;

        if (args.FatherAppId == id)
        {
            throw new InvalidOperationException("An app cannot be its own father");
        }

        if (args.FatherAppId.HasValue)
        {
            var fatherInTenant = await appRepository
                .GetExistingIdsInTenantAsync([args.FatherAppId.Value], tenantId)
                .ConfigureAwait(false);
            if (fatherInTenant.Count == 0)
            {
                throw new InvalidOperationException("Father app doesn't belong to the tenant");
            }

            // Re-parenting under a descendant would create a cycle.
            var fatherAncestors = await appRepository
                .GetAncestorIdsAsync(args.FatherAppId.Value, tenantId)
                .ConfigureAwait(false);
            if (fatherAncestors.Contains(id))
            {
                throw new InvalidOperationException("Re-parenting would create a cycle");
            }
        }

        await appRepository
            .UpdateAndSaveFatherByIdAsync(
            id,
            args.FatherAppId,
            tenantId)
            .ConfigureAwait(false);
    }

    public async Task<List<App>> GetByEmailAccountAsync(string email)
    {
        var apps = await appRepository
            .GetByEmailAccountAsync(email)
            .ConfigureAwait(false);

        return apps;
    }
}
