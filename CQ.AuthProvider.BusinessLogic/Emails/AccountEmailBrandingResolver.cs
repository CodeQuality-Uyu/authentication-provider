using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.Blobs;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.Emails;

internal sealed class AccountEmailBrandingResolver(
    IAppRepository _appRepository,
    IBlobService _blobService)
    : IAccountEmailBrandingResolver
{
    public async Task<string?> GetLogoUrlAsync(Guid? appId, Guid tenantId)
    {
        var app = await GetBrandingAppAsync(appId, tenantId)
            .ConfigureAwait(false);

        // The light-background variant: mails are rendered on white.
        var logoKey = app?.Logo?.LightKey;
        if (Guard.IsNullOrEmpty(logoKey))
        {
            return null;
        }

        // NOTE: GetByKey returns a non-expiring CDN URL only when Blob:CloudFrontDomain is set.
        // Without it, the URL is presigned (Blob:PresignedUrlExpiration, 15 min by default), and
        // a mail opened later than that shows a broken image. Set CloudFrontDomain in every
        // environment that sends mails.
        var logo = _blobService.GetByKey(logoKey!);

        return logo.Url;
    }

    private async Task<App?> GetBrandingAppAsync(Guid? appId, Guid tenantId)
    {
        if (appId is not null)
        {
            // Only apps of the account's tenant: the app id may come from the request (reset
            // password), and another tenant's logo must never brand this tenant's mail. Checked
            // first because GetByIdAsync throws for an unknown id, and branding must not break
            // the mail.
            var appsInTenant = await _appRepository
                .GetExistingIdsInTenantAsync([appId.Value], tenantId)
                .ConfigureAwait(false);

            if (appsInTenant.Count != 0)
            {
                return await _appRepository
                    .GetByIdAsync(appId.Value)
                    .ConfigureAwait(false);
            }
        }

        // No app, or one of another tenant: the tenant's default app, which may not exist.
        return await _appRepository
            .GetOrDefaultByDefaultAsync(tenantId)
            .ConfigureAwait(false);
    }
}
