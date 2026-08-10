using CQ.AuthProvider.BusinessLogic.Blobs;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.Emails;

internal sealed class AccountEmailBrandingResolver(
    ITenantRepository _tenantRepository,
    IBlobService _blobService)
    : IAccountEmailBrandingResolver
{
    public async Task<string?> GetLogoUrlAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository
            .GetByIdAsync(tenantId)
            .ConfigureAwait(false);

        if (Guard.IsNullOrEmpty(tenant.MiniLogoKey))
        {
            return null;
        }

        // NOTE: GetByKey returns a presigned URL (~15 min TTL, see AWSBlobService) — good enough
        // for now, but a mail opened later than that will show a broken image. Follow-up: give
        // IBlobService a non-expiring variant (public bucket path / CDN URL) for this use case,
        // same split ecolors-web-api's IBlobService already has (GetByKey vs GetPresignedByKey).
        var logo = _blobService.GetByKey(tenant.MiniLogoKey!);

        return logo.Url;
    }
}
