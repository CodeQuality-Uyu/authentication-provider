using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.AuthProvider.DataAccess.EfCore.Permissions;
using CQ.AuthProvider.Tests.Infrastructure;
using CQ.UnitOfWork.EfCore.Core;

namespace CQ.AuthProvider.Tests;

/// <summary>
/// El filtro <c>search</c> de <c>GET /permissions</c>: un solo texto que se busca en el nombre
/// <b>o</b> en la key.
/// </summary>
/// <remarks>
/// Reemplaza a <c>name</c> y <c>key</c>, que eran filtros separados combinados con AND: con un
/// solo buscador no se podía encontrar un permiso escribiendo su key y su nombre a la vez.
/// </remarks>
public sealed class PermissionSearchTests
{
    [Fact]
    public async Task Finds_by_name()
    {
        using var fixture = new AuthDbContextFixture();
        var data = await SeedAsync(fixture);

        var ids = await SearchAsync(fixture, "uso de categorías");

        Assert.Equal([data.UsageId], ids);
    }

    [Fact]
    public async Task Finds_by_key()
    {
        using var fixture = new AuthDbContextFixture();
        var data = await SeedAsync(fixture);

        var ids = await SearchAsync(fixture, "getusage-category");

        Assert.Equal([data.UsageId], ids);
    }

    [Fact]
    public async Task Matches_name_or_key_in_the_same_search()
    {
        using var fixture = new AuthDbContextFixture();
        var data = await SeedAsync(fixture);

        // "banner" está en la key de uno y sólo en el nombre del otro.
        var ids = await SearchAsync(fixture, "banner");

        Assert.Equal(
            new[] { data.BannerKeyId, data.BannerNameId }.Order(),
            ids.Order());
    }

    [Fact]
    public async Task Ignores_case_and_surrounding_spaces()
    {
        using var fixture = new AuthDbContextFixture();
        var data = await SeedAsync(fixture);

        var ids = await SearchAsync(fixture, "  GETUSAGE  ");

        Assert.Equal([data.UsageId], ids);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Empty_search_does_not_filter(string? search)
    {
        using var fixture = new AuthDbContextFixture();
        var data = await SeedAsync(fixture);

        var ids = await SearchAsync(fixture, search);

        // La base también trae los permisos sembrados del tenant de seed: sin filtro, son todos.
        Assert.Superset(
            new HashSet<Guid> { data.UsageId, data.BannerKeyId, data.BannerNameId },
            ids.ToHashSet());
        Assert.Equal(fixture.Context.Permissions.Count(), ids.Count);
    }

    [Fact]
    public async Task Combines_with_role_and_app()
    {
        using var fixture = new AuthDbContextFixture();
        var data = await SeedAsync(fixture);

        var ofRole = await SearchAsync(fixture, "banner", roleId: data.RoleId);
        var ofOtherApp = await SearchAsync(fixture, "banner", appId: data.OtherAppId);

        Assert.Equal([data.BannerKeyId], ofRole);
        Assert.Equal([data.BannerNameId], ofOtherApp);
    }

    /// <summary>
    /// Tres permisos en dos apps del tenant de seed, y un rol con uno solo de ellos.
    /// </summary>
    private static async Task<(Guid UsageId, Guid BannerKeyId, Guid BannerNameId, Guid RoleId, Guid OtherAppId)> SeedAsync(
        AuthDbContextFixture fixture)
    {
        var tree = TestTree.CreateIn(fixture.Context);

        var usage = TestTree.NewPermission(tree.ChildId, "getusage-category", isPublic: true);
        usage.Name = "Ver uso de categorías";
        var bannerKey = TestTree.NewPermission(tree.ChildId, "getall-banner", isPublic: true);
        bannerKey.Name = "Ver carteles";
        var bannerName = TestTree.NewPermission(tree.UnrelatedId, "create-promo", isPublic: true);
        bannerName.Name = "Crear banner promocional";

        var role = TestTree.NewRole(tree.ChildId, "Licenciatario", isPublic: false);
        role.Permissions.Add(bannerKey);

        fixture.Context.Permissions.AddRange(usage, bannerKey, bannerName);
        fixture.Context.Roles.Add(role);
        await fixture.Context.SaveChangesAsync();

        return (usage.Id, bannerKey.Id, bannerName.Id, role.Id, tree.UnrelatedId);
    }

    private static async Task<List<Guid>> SearchAsync(
        AuthDbContextFixture fixture,
        string? search,
        Guid? roleId = null,
        Guid? appId = null)
    {
        var repository = new PermissionRepository(
            fixture.Context,
            DataAccessMapper.Create(),
            new EfCoreRepository<PermissionApp>(fixture.Context));

        var page = await repository.GetAllAsync(
            appId,
            isPrivate: null,
            roleId,
            search,
            page: 1,
            pageSize: 100,
            Logged());

        return page.Items.Select(p => p.Id).ToList();
    }

    /// <summary>Una cuenta del tenant de seed, logueada en una app que no es la del Auth Provider.</summary>
    private static AccountLogged Logged()
    {
        return new AccountLogged
        {
            Tenant = new Tenant { Id = AuthConstants.SEED_TENANT_ID },
            AppLogged = new App { Id = Guid.NewGuid() },
        };
    }
}
