using CQ.AuthProvider.DataAccess.EfCore.Apps;
using CQ.AuthProvider.DataAccess.EfCore.Permissions;
using CQ.AuthProvider.DataAccess.EfCore.Roles;
using CQ.AuthProvider.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CQ.AuthProvider.Tests;

/// <summary>
/// La regla de <see cref="EffectiveScope"/>: qué roles y permisos aplican a una app.
/// </summary>
/// <remarks>
/// Corren contra SQLite, así que además de la regla prueban que las queries <b>traduzcan</b>: los
/// subqueries correlacionados sobre AppsAncestors y sobre los grants son la parte del cambio que no
/// se puede verificar leyendo el código.
/// </remarks>
public sealed class EffectiveScopeTests
{
    [Fact]
    public async Task Role_of_the_app_itself_is_effective()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var own = TestTree.NewRole(tree.ChildId, "own", isPublic: false);
        fixture.Context.Roles.Add(own);
        await fixture.Context.SaveChangesAsync();

        var effective = await EffectiveRoleIdsAsync(fixture, tree.ChildId);

        Assert.Contains(own.Id, effective);
    }

    [Fact]
    public async Task Public_role_of_an_ancestor_is_inherited()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var parentPublic = TestTree.NewRole(tree.ParentId, "parent-public", isPublic: true);
        var grandparentPublic = TestTree.NewRole(tree.GrandparentId, "grandparent-public", isPublic: true);
        fixture.Context.Roles.AddRange(parentPublic, grandparentPublic);
        await fixture.Context.SaveChangesAsync();

        var effective = await EffectiveRoleIdsAsync(fixture, tree.ChildId);

        Assert.Contains(parentPublic.Id, effective);

        // Depth 2: la herencia sube todo el árbol, no solo al padre directo — que es lo único que
        // alcanzaba el filtro viejo (r.AppId == s.App.FatherAppId).
        Assert.Contains(grandparentPublic.Id, effective);
    }

    [Fact]
    public async Task Private_role_of_an_ancestor_is_not_inherited()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var parentPrivate = TestTree.NewRole(tree.ParentId, "parent-private", isPublic: false);
        fixture.Context.Roles.Add(parentPrivate);
        await fixture.Context.SaveChangesAsync();

        var effective = await EffectiveRoleIdsAsync(fixture, tree.ChildId);

        Assert.DoesNotContain(parentPrivate.Id, effective);
    }

    [Fact]
    public async Task Private_role_of_an_ancestor_with_an_explicit_grant_is_effective()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var parentPrivate = TestTree.NewRole(tree.ParentId, "parent-private", isPublic: false);
        fixture.Context.Roles.Add(parentPrivate);
        fixture.Context.RolesApps.Add(new RoleApp
        {
            RoleId = parentPrivate.Id,
            AppId = tree.ChildId,
        });
        await fixture.Context.SaveChangesAsync();

        var effective = await EffectiveRoleIdsAsync(fixture, tree.ChildId);

        Assert.Contains(parentPrivate.Id, effective);
    }

    [Fact]
    public async Task Grant_reaches_only_the_app_it_was_given_to()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var grandparentPrivate = TestTree.NewRole(tree.GrandparentId, "grandparent-private", isPublic: false);
        fixture.Context.Roles.Add(grandparentPrivate);
        fixture.Context.RolesApps.Add(new RoleApp
        {
            RoleId = grandparentPrivate.Id,
            AppId = tree.ChildId,
        });
        await fixture.Context.SaveChangesAsync();

        Assert.Contains(grandparentPrivate.Id, await EffectiveRoleIdsAsync(fixture, tree.ChildId));

        // El grant es a Child, no a Parent: que Parent esté en el medio del árbol no lo alcanza.
        Assert.DoesNotContain(grandparentPrivate.Id, await EffectiveRoleIdsAsync(fixture, tree.ParentId));
    }

    [Fact]
    public async Task Role_of_an_unrelated_app_is_never_effective()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var unrelatedPublic = TestTree.NewRole(tree.UnrelatedId, "unrelated-public", isPublic: true);
        fixture.Context.Roles.Add(unrelatedPublic);
        await fixture.Context.SaveChangesAsync();

        var effective = await EffectiveRoleIdsAsync(fixture, tree.ChildId);

        // Mismo tenant, público, pero no es ancestro: ser público no alcanza, hay que estar arriba
        // en la rama.
        Assert.DoesNotContain(unrelatedPublic.Id, effective);
    }

    [Fact]
    public async Task Inheritance_does_not_go_downwards()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var childPublic = TestTree.NewRole(tree.ChildId, "child-public", isPublic: true);
        fixture.Context.Roles.Add(childPublic);
        await fixture.Context.SaveChangesAsync();

        var effective = await EffectiveRoleIdsAsync(fixture, tree.ParentId);

        Assert.DoesNotContain(childPublic.Id, effective);
    }

    [Fact]
    public async Task Permissions_follow_the_same_rule()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var parentPublic = TestTree.NewPermission(tree.ParentId, "parent-public-permission", isPublic: true);
        var parentPrivate = TestTree.NewPermission(tree.ParentId, "parent-private-permission", isPublic: false);
        var granted = TestTree.NewPermission(tree.ParentId, "granted-permission", isPublic: false);
        fixture.Context.Permissions.AddRange(parentPublic, parentPrivate, granted);
        fixture.Context.PermissionsApps.Add(new PermissionApp
        {
            PermissionId = granted.Id,
            AppId = tree.ChildId,
        });
        await fixture.Context.SaveChangesAsync();

        var effective = await fixture
            .Context
            .Permissions
            .EffectiveForApp(fixture.Context, tree.ChildId)
            .Select(p => p.Id)
            .ToListAsync();

        Assert.Contains(parentPublic.Id, effective);
        Assert.Contains(granted.Id, effective);
        Assert.DoesNotContain(parentPrivate.Id, effective);
    }

    [Fact]
    public async Task EffectiveForAnyApp_is_the_union_over_the_apps()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var childOwn = TestTree.NewRole(tree.ChildId, "child-own", isPublic: false);
        var unrelatedOwn = TestTree.NewRole(tree.UnrelatedId, "unrelated-own", isPublic: false);
        var parentPublic = TestTree.NewRole(tree.ParentId, "parent-public", isPublic: true);
        fixture.Context.Roles.AddRange(childOwn, unrelatedOwn, parentPublic);
        await fixture.Context.SaveChangesAsync();

        var effective = await fixture
            .Context
            .Roles
            .EffectiveForAnyApp(fixture.Context, [tree.ChildId, tree.UnrelatedId])
            .Select(r => r.Id)
            .ToListAsync();

        Assert.Contains(childOwn.Id, effective);
        Assert.Contains(unrelatedOwn.Id, effective);
        Assert.Contains(parentPublic.Id, effective);
    }

    private static async Task<List<Guid>> EffectiveRoleIdsAsync(
        AuthDbContextFixture fixture,
        Guid appId)
    {
        return await fixture
            .Context
            .Roles
            .EffectiveForApp(fixture.Context, appId)
            .Select(r => r.Id)
            .ToListAsync();
    }
}
