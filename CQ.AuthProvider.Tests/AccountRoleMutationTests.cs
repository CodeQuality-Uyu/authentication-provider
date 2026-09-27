using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.AuthProvider.DataAccess.EfCore.Accounts;
using CQ.AuthProvider.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CQ.AuthProvider.Tests;

/// <summary>
/// Que agregar y quitar roles toque la cuenta que se pidió y no otra.
/// </summary>
/// <remarks>
/// Regresión de un bug real: <c>DeleteRolesByIdAsync</c> y <c>AddRolesByIdAsync</c> recibían el
/// <c>AccountLogged</c> y filtraban por <c>accountLogged.Id</c>, así que
/// <c>PATCH /accounts/{id}/roles</c> ignoraba el <c>{id}</c> y le modificaba los roles a la cuenta
/// logueada. Además de no hacer lo que dice el endpoint, era una escalada de privilegios:
/// cualquiera con <c>updateroles-account</c> se asignaba a sí mismo cualquier rol del tenant.
/// </remarks>
public sealed class AccountRoleMutationTests
{
    [Fact]
    public async Task Adding_roles_touches_only_the_requested_account()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var role = TestTree.NewRole(tree.ChildId, "target-role", isPublic: false);
        fixture.Context.Roles.Add(role);

        var target = NewAccount("target@test.com");
        var caller = NewAccount("caller@test.com");
        fixture.Context.Accounts.AddRange(target, caller);
        await fixture.Context.SaveChangesAsync();

        var repository = NewRepository(fixture);

        await repository.AddRolesByIdAsync(target.Id, [role.Id]);
        await fixture.Context.SaveChangesAsync();

        Assert.Equal([role.Id], await RoleIdsOfAsync(fixture, target.Id));
        Assert.Empty(await RoleIdsOfAsync(fixture, caller.Id));
    }

    [Fact]
    public async Task Removing_roles_touches_only_the_requested_account()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var shared = TestTree.NewRole(tree.ChildId, "shared-role", isPublic: false);
        fixture.Context.Roles.Add(shared);

        var target = NewAccount("target@test.com");
        var caller = NewAccount("caller@test.com");
        fixture.Context.Accounts.AddRange(target, caller);
        await fixture.Context.SaveChangesAsync();

        // Las dos cuentas tienen el mismo rol: si la baja se fuera por la cuenta equivocada, el
        // test lo ve.
        fixture.Context.AccountsRoles.AddRange(
            new AccountRole { AccountId = target.Id, RoleId = shared.Id },
            new AccountRole { AccountId = caller.Id, RoleId = shared.Id });
        await fixture.Context.SaveChangesAsync();

        var repository = NewRepository(fixture);

        await repository.DeleteRolesByIdAsync(target.Id, [shared.Id]);
        await fixture.Context.SaveChangesAsync();

        Assert.Empty(await RoleIdsOfAsync(fixture, target.Id));
        Assert.Equal([shared.Id], await RoleIdsOfAsync(fixture, caller.Id));
    }

    [Fact]
    public async Task Roles_snapshot_returns_the_tenant_and_every_role_regardless_of_app()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);

        var childRole = TestTree.NewRole(tree.ChildId, "child-role", isPublic: false);
        var unrelatedRole = TestTree.NewRole(tree.UnrelatedId, "unrelated-role", isPublic: false);
        fixture.Context.Roles.AddRange(childRole, unrelatedRole);

        var account = NewAccount("account@test.com");
        fixture.Context.Accounts.Add(account);
        await fixture.Context.SaveChangesAsync();

        fixture.Context.AccountsRoles.AddRange(
            new AccountRole { AccountId = account.Id, RoleId = childRole.Id },
            new AccountRole { AccountId = account.Id, RoleId = unrelatedRole.Id });
        await fixture.Context.SaveChangesAsync();

        var snapshot = await NewRepository(fixture).GetRolesSnapshotByIdAsync(account.Id);

        Assert.NotNull(snapshot);
        Assert.Equal(AuthConstants.SEED_TENANT_ID, snapshot.Value.TenantId);

        // Sin filtrar por app a propósito: es lo que UpdateRolesAsync usa para saber qué roles ya
        // tiene la cuenta y no volver a agregarlos.
        Assert.Equal(2, snapshot.Value.RoleIds.Count);
        Assert.Contains(childRole.Id, snapshot.Value.RoleIds);
        Assert.Contains(unrelatedRole.Id, snapshot.Value.RoleIds);
    }

    [Fact]
    public async Task Roles_snapshot_of_a_missing_account_is_null()
    {
        using var fixture = new AuthDbContextFixture();

        var snapshot = await NewRepository(fixture).GetRolesSnapshotByIdAsync(Guid.NewGuid());

        Assert.Null(snapshot);
    }

    private static AccountRepository NewRepository(AuthDbContextFixture fixture)
    {
        return new AccountRepository(fixture.Context, DataAccessMapper.Create());
    }

    private static AccountEfCore NewAccount(string email)
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
            TenantId = AuthConstants.SEED_TENANT_ID,
        };
    }

    private static async Task<List<Guid>> RoleIdsOfAsync(
        AuthDbContextFixture fixture,
        Guid accountId)
    {
        return await fixture
            .Context
            .AccountsRoles
            .Where(ar => ar.AccountId == accountId)
            .Select(ar => ar.RoleId)
            .ToListAsync();
    }
}
