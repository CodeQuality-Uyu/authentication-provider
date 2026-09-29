using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Permissions;
using CQ.AuthProvider.BusinessLogic.Roles;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.AuthProvider.DataAccess.EfCore.Accounts;
using CQ.AuthProvider.DataAccess.EfCore.Tenants;
using CQ.AuthProvider.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CQ.AuthProvider.Tests;

/// <summary>
/// La vista global (<c>getallcrosstenant-account</c>) sobre cuentas, y el tenant como límite para
/// quien no la tiene.
/// </summary>
/// <remarks>
/// Incluye la regresión de <c>GET /accounts/{id}</c>, que no validaba el tenant: con
/// <c>getall-account</c> y el id se leía una cuenta de cualquier tenant.
/// </remarks>
public sealed class CrossTenantAccountTests
{
    private static readonly Guid OtherTenantId = Guid.NewGuid();

    [Fact]
    public async Task Listing_without_tenant_returns_accounts_of_every_tenant_with_apps_and_tenant()
    {
        using var fixture = new AuthDbContextFixture();
        var data = await SeedTwoTenantsAsync(fixture);

        var page = await NewRepository(fixture).GetAllAsync(null, null, 1, 100);

        var own = Assert.Single(page.Items, a => a.Id == data.OwnAccountId);
        var other = Assert.Single(page.Items, a => a.Id == data.OtherAccountId);

        Assert.Equal(AuthConstants.SEED_TENANT_ID, own.Tenant.Id);
        Assert.Equal("Other tenant", other.Tenant.Name);
        Assert.Equal(["Other app"], other.Apps.ConvertAll(a => a.Name));
    }

    [Fact]
    public async Task Listing_with_tenant_returns_only_that_tenant()
    {
        using var fixture = new AuthDbContextFixture();
        var data = await SeedTwoTenantsAsync(fixture);

        var page = await NewRepository(fixture).GetAllAsync(OtherTenantId, null, 1, 100);

        var account = Assert.Single(page.Items);
        Assert.Equal(data.OtherAccountId, account.Id);
    }

    [Fact]
    public async Task Listing_without_tenant_filters_by_an_app_of_any_tenant()
    {
        using var fixture = new AuthDbContextFixture();
        var data = await SeedTwoTenantsAsync(fixture);

        var page = await NewRepository(fixture).GetAllAsync(null, data.OtherAppId, 1, 100);

        var account = Assert.Single(page.Items);
        Assert.Equal(data.OtherAccountId, account.Id);
    }

    [Fact]
    public async Task Detail_of_an_account_of_another_tenant_is_not_found()
    {
        using var fixture = new AuthDbContextFixture();
        var data = await SeedTwoTenantsAsync(fixture);

        await Assert.ThrowsAnyAsync<Exception>(() => NewRepository(fixture).GetByIdAsync(
            data.OtherAccountId,
            AuthConstants.AUTH_WEB_API_APP_ID,
            AuthConstants.SEED_TENANT_ID));
    }

    [Fact]
    public async Task Detail_without_tenant_reads_an_account_of_any_tenant()
    {
        using var fixture = new AuthDbContextFixture();
        var data = await SeedTwoTenantsAsync(fixture);

        var account = await NewRepository(fixture).GetByIdAsync(
            data.OtherAccountId,
            AuthConstants.AUTH_WEB_API_APP_ID,
            null);

        Assert.Equal(data.OtherAccountId, account.Id);
    }

    [Fact]
    public void Without_the_global_view_the_filter_is_always_the_own_tenant()
    {
        var logged = NewLogged(withGlobalView: false);

        Assert.Equal(AuthConstants.SEED_TENANT_ID, logged.ResolveTenantFilter(null));
        Assert.Equal(AuthConstants.SEED_TENANT_ID, logged.ResolveTenantFilter(AuthConstants.SEED_TENANT_ID));
    }

    [Fact]
    public void Without_the_global_view_asking_for_another_tenant_is_denied()
    {
        var logged = NewLogged(withGlobalView: false);

        var exception = Assert.Throws<CrossTenantAccessDeniedException>(
            () => logged.ResolveTenantFilter(OtherTenantId));
        Assert.Equal(OtherTenantId, exception.TenantId);
    }

    [Fact]
    public void With_the_global_view_the_filter_is_the_one_asked_for()
    {
        var logged = NewLogged(withGlobalView: true);

        Assert.Null(logged.ResolveTenantFilter(null));
        Assert.Equal(OtherTenantId, logged.ResolveTenantFilter(OtherTenantId));
    }

    [Fact]
    public async Task The_global_view_is_seeded_private_and_only_in_the_auth_web_api_owner_role()
    {
        using var fixture = new AuthDbContextFixture();

        var permission = await fixture.Context.Permissions.SingleAsync(
            p => p.Key == AuthConstants.GET_ALL_CROSS_TENANT_ACCOUNT_PERMISSION_KEY);
        Assert.False(permission.IsPublic);

        var roleIds = await fixture
            .Context
            .RolesPermissions
            .Where(rp => rp.PermissionId == permission.Id)
            .Select(rp => rp.RoleId)
            .ToListAsync();
        Assert.Equal([AuthConstants.AUTH_WEB_API_OWNER_ROLE_ID], roleIds);
    }

    private static async Task<(Guid OwnAccountId, Guid OtherAccountId, Guid OtherAppId)> SeedTwoTenantsAsync(
        AuthDbContextFixture fixture)
    {
        fixture.Context.Tenants.Add(new TenantEfCore { Id = OtherTenantId, Name = "Other tenant" });

        var otherApp = TestTree.NewApp(Guid.NewGuid(), "Other app", null) with { TenantId = OtherTenantId };
        fixture.Context.Apps.Add(otherApp);

        var own = NewAccount("own@test.com", AuthConstants.SEED_TENANT_ID);
        var other = NewAccount("other@test.com", OtherTenantId);
        fixture.Context.Accounts.AddRange(own, other);
        await fixture.Context.SaveChangesAsync();

        fixture.Context.AccountsApps.Add(new AccountApp { AccountId = other.Id, AppId = otherApp.Id });
        await fixture.Context.SaveChangesAsync();

        return (own.Id, other.Id, otherApp.Id);
    }

    private static AccountLogged NewLogged(bool withGlobalView)
    {
        List<Permission> permissions = withGlobalView
            ? [new Permission { Key = AuthConstants.GET_ALL_CROSS_TENANT_ACCOUNT_PERMISSION_KEY }]
            : [new Permission { Key = "getall-account" }];

        return new AccountLogged
        {
            Tenant = new Tenant { Id = AuthConstants.SEED_TENANT_ID },
            Roles = [new Role { Permissions = permissions }],
        };
    }

    private static AccountRepository NewRepository(AuthDbContextFixture fixture)
    {
        return new AccountRepository(fixture.Context, DataAccessMapper.Create());
    }

    private static AccountEfCore NewAccount(
        string email,
        Guid tenantId)
    {
        return new AccountEfCore
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "Test",
            LastName = "Account",
            FullName = "Test Account",
            Locale = "Uruguay",
            TimeZone = "-3",
            TenantId = tenantId,
        };
    }
}
