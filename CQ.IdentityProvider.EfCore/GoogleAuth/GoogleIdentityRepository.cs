using CQ.AuthProvider.BusinessLogic.GoogleAuth;
using Microsoft.EntityFrameworkCore;

namespace CQ.IdentityProvider.EfCore.GoogleAuth;

internal sealed class GoogleIdentityRepository(IdentityDbContext context)
    : IGoogleIdentityRepository
{
    public async Task CreateAndSaveAsync(GoogleIdentity identity)
    {
        await context
            .GoogleIdentities
            .AddAsync(identity)
            .ConfigureAwait(false);

        await context
            .SaveChangesAsync()
            .ConfigureAwait(false);
    }

    public async Task<GoogleIdentity?> GetByGoogleSubAsync(string googleSub)
    {
        return await context
            .GoogleIdentities
            .FirstOrDefaultAsync(g => g.GoogleSub == googleSub)
            .ConfigureAwait(false);
    }
}
