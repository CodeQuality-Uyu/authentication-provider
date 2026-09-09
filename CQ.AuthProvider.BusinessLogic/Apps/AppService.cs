using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Blobs;
using CQ.AuthProvider.BusinessLogic.Subscriptions;
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
        
        args.Logo.ColorKey = await blobService
            .MoveObjectAsync(
                args.Logo.ColorKey,
                accountLogged.AppLogged.Name,
                args.Name)
            .ConfigureAwait(false);

        args.Logo.LightKey = await blobService
            .MoveObjectAsync(
                args.Logo.LightKey,
                accountLogged.AppLogged.Name,
                args.Name)
            .ConfigureAwait(false);

        args.Logo.DarkKey = await blobService
            .MoveObjectAsync(
                args.Logo.DarkKey,
                accountLogged.AppLogged.Name,
                args.Name)
            .ConfigureAwait(false);

        if(Guard.IsNotNullOrEmpty(args.Background?.BackgroundKey))
        {
            args.Background.BackgroundKey = await blobService
            .MoveObjectAsync(
                args.Background.BackgroundKey!,
                accountLogged.AppLogged.Name,
                args.Name)
            .ConfigureAwait(false);
        }

        var app = new App(
            args.Name,
            args.IsDefault,
            args.Logo,
            args.Background,
            accountLogged.Tenant,
            null,
            args.AccountDataSource,
            args.GoogleClientId);

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

        if (Guard.IsNotNull(args.Logo))
        {
            args.Logo.ColorKey = await blobService
                .MoveObjectAsync(
                args.Logo.ColorKey,
                accountLogged.AppLogged.Name,
                args.Name)
                .ConfigureAwait(false);

            args.Logo.LightKey = await blobService
                .MoveObjectAsync(
                args.Logo.LightKey,
                accountLogged.AppLogged.Name,
                args.Name)
                .ConfigureAwait(false);

            args.Logo.DarkKey = await blobService
                .MoveObjectAsync(
                args.Logo.DarkKey,
                accountLogged.AppLogged.Name,
                args.Name)
                .ConfigureAwait(false);
        }

        if (Guard.IsNotNullOrEmpty(args.Background?.BackgroundKey))
        {
            args.Background.BackgroundKey = await blobService
            .MoveObjectAsync(
                args.Background.BackgroundKey!,
                accountLogged.AppLogged.Name,
                args.Name)
            .ConfigureAwait(false);
        }

        var app = new App(
            args.Name,
            false,
            args.Logo ?? accountLogged.AppLogged.Logo,
            args.Background,
            accountLogged.Tenant,
            accountLogged.AppLogged,
            args.AccountDataSource,
            args.GoogleClientId);

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

    public async Task UpdateColorsByIdAsync(
        Guid id,
        Background args,
        AccountLogged accountLogged)
    {
        var appIsNotOfAccount = !accountLogged.AppsIds.Contains(id);
        if (appIsNotOfAccount)
        {
            throw new InvalidOperationException("Account doesn't belong to app");
        }

        await appRepository
            .UpdateAndSaveColorsByIdAsync(id, args)
            .ConfigureAwait(false);
    }

    public async Task UpdateByIdAsync(
        Guid id,
        UpdateAppArgs args,
        AccountLogged accountLogged)
    {
        var appIsNotOfAccount = !accountLogged.AppsIds.Contains(id);
        if (appIsNotOfAccount)
        {
            throw new InvalidOperationException("Account doesn't belong to app");
        }

        await appRepository
            .UpdateAndSaveByIdAsync(id, args.Name, args.AccountDataSource, args.GoogleClientId)
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
