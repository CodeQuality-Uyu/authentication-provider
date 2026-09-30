using CQ.AuthProvider.BusinessLogic.Utils;
using Microsoft.EntityFrameworkCore;

namespace CQ.AuthProvider.Tests.Infrastructure;

/// <summary>Verifica que el fixture levante el esquema y el seed antes de probar reglas.</summary>
public sealed class SmokeTests
{
    [Fact]
    public async Task Schema_and_seed_are_created()
    {
        using var fixture = new AuthDbContextFixture();

        var authApp = await fixture
            .Context
            .Apps
            .FirstOrDefaultAsync(a => a.Id == AuthConstants.AUTH_WEB_API_APP_ID);

        Assert.NotNull(authApp);
        Assert.True(await fixture.Context.Roles.AnyAsync());
        Assert.True(await fixture.Context.Permissions.AnyAsync());
    }
}
