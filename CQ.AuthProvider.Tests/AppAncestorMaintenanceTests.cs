using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.AuthProvider.DataAccess.EfCore.Apps;
using CQ.AuthProvider.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CQ.AuthProvider.Tests;

/// <summary>
/// El mantenimiento de la tabla de cierre <see cref="AppAncestor"/> en <c>AppRepository</c>.
/// </summary>
/// <remarks>
/// La tabla es un índice derivado de <c>Apps.FatherAppId</c>. Si se desincroniza, la herencia de
/// roles y permisos da resultados mal sin que falle nada, así que el mantenimiento es lo que más
/// vale probar de todo el cambio.
/// </remarks>
public sealed class AppAncestorMaintenanceTests
{
    [Fact]
    public async Task Creating_an_app_writes_the_whole_ancestor_chain()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var repository = NewRepository(fixture);

        var grandchildId = Guid.NewGuid();

        await repository.CreateAsync(new App
        {
            Id = grandchildId,
            Name = "Grandchild",
            Tenant = new BusinessLogic.Tenants.Tenant { Id = AuthConstants.SEED_TENANT_ID },
            Logo = new Logo { ColorKey = "c.png", LightKey = "l.png", DarkKey = "d.png" },
            FatherApp = new App { Id = tree.ChildId },
        });

        await fixture.Context.SaveChangesAsync();

        var ancestors = await AncestorsOfAsync(fixture, grandchildId);

        // Padre, abuelo y bisabuelo: la cadena entera, no solo el padre directo.
        Assert.Equal(3, ancestors.Count);
        Assert.Equal(1, ancestors[tree.ChildId]);
        Assert.Equal(2, ancestors[tree.ParentId]);
        Assert.Equal(3, ancestors[tree.GrandparentId]);
    }

    [Fact]
    public async Task Re_parenting_recomputes_the_subtree_and_drops_the_old_ancestors()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var repository = NewRepository(fixture);

        // Child pasa de estar bajo Parent a estar bajo Unrelated, que es raíz.
        await repository.UpdateAndSaveFatherByIdAsync(
            tree.ChildId,
            tree.UnrelatedId,
            AuthConstants.SEED_TENANT_ID);

        var ancestors = await AncestorsOfAsync(fixture, tree.ChildId);

        Assert.Single(ancestors);
        Assert.Equal(1, ancestors[tree.UnrelatedId]);

        // Lo que importa: los ancestros viejos se fueron. Si quedaran, Child seguiría heredando
        // los roles públicos de una rama a la que ya no pertenece.
        Assert.DoesNotContain(tree.ParentId, ancestors.Keys);
        Assert.DoesNotContain(tree.GrandparentId, ancestors.Keys);
    }

    [Fact]
    public async Task Re_parenting_updates_the_depth_of_the_descendants()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var repository = NewRepository(fixture);

        // Parent deja de ser hijo de Grandparent y pasa a ser raíz. Child es descendiente de
        // Parent, así que su cierre también tiene que cambiar aunque no se lo haya tocado.
        await repository.UpdateAndSaveFatherByIdAsync(
            tree.ParentId,
            null,
            AuthConstants.SEED_TENANT_ID);

        var parentAncestors = await AncestorsOfAsync(fixture, tree.ParentId);
        Assert.Empty(parentAncestors);

        var childAncestors = await AncestorsOfAsync(fixture, tree.ChildId);
        Assert.Single(childAncestors);
        Assert.Equal(1, childAncestors[tree.ParentId]);
    }

    [Fact]
    public async Task Re_parenting_deeper_in_the_same_branch_shifts_the_depths()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var repository = NewRepository(fixture);

        // Child pasa de colgar de Parent a colgar de Grandparent: el par (Child, Grandparent)
        // sobrevive pero cambia de profundidad 2 a 1. Es el caso que el diff tiene que actualizar
        // en vez de dejar como estaba.
        await repository.UpdateAndSaveFatherByIdAsync(
            tree.ChildId,
            tree.GrandparentId,
            AuthConstants.SEED_TENANT_ID);

        var ancestors = await AncestorsOfAsync(fixture, tree.ChildId);

        Assert.Single(ancestors);
        Assert.Equal(1, ancestors[tree.GrandparentId]);
    }

    [Fact]
    public async Task GetDescendantIds_only_returns_apps_below_the_ancestor()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var repository = NewRepository(fixture);

        var candidates = new List<Guid> { tree.ChildId, tree.UnrelatedId, tree.GrandparentId };

        var descendants = await repository.GetDescendantIdsAsync(candidates, tree.ParentId);

        // Es lo que usa la validación de los grants: solo Child está debajo de Parent, así que un
        // grant hacia Unrelated o hacia el propio abuelo se rechaza.
        Assert.Equal([tree.ChildId], descendants);
    }

    private static AppRepository NewRepository(AuthDbContextFixture fixture)
    {
        return new AppRepository(fixture.Context, DataAccessMapper.Create());
    }

    private static async Task<Dictionary<Guid, int>> AncestorsOfAsync(
        AuthDbContextFixture fixture,
        Guid appId)
    {
        return await fixture
            .Context
            .AppsAncestors
            .Where(aa => aa.AppId == appId)
            .ToDictionaryAsync(aa => aa.AncestorId, aa => aa.Depth);
    }
}
