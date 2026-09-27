using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.AuthProvider.DataAccess.EfCore;
using CQ.AuthProvider.DataAccess.EfCore.Apps;
using CQ.AuthProvider.DataAccess.EfCore.Permissions;
using CQ.AuthProvider.DataAccess.EfCore.Roles;

namespace CQ.AuthProvider.Tests.Infrastructure;

/// <summary>
/// Árbol de apps de tres niveles más una app suelta, para probar el alcance efectivo.
/// </summary>
/// <remarks>
/// <c>Grandparent → Parent → Child</c>, y <c>Unrelated</c> aparte en el mismo tenant. Las filas de
/// <see cref="AppAncestor"/> se escriben a mano acá: esta clase representa el estado que
/// <c>AppRepository</c> debería mantener, y que ese mantenimiento sea correcto se prueba por
/// separado en <see cref="AppAncestorMaintenanceTests"/>.
/// </remarks>
internal sealed class TestTree
{
    internal Guid GrandparentId { get; } = Guid.NewGuid();

    internal Guid ParentId { get; } = Guid.NewGuid();

    internal Guid ChildId { get; } = Guid.NewGuid();

    internal Guid UnrelatedId { get; } = Guid.NewGuid();

    internal static TestTree CreateIn(AuthDbContext context)
    {
        var tree = new TestTree();

        context.Apps.AddRange(
            NewApp(tree.GrandparentId, "Grandparent", null),
            NewApp(tree.ParentId, "Parent", tree.GrandparentId),
            NewApp(tree.ChildId, "Child", tree.ParentId),
            NewApp(tree.UnrelatedId, "Unrelated", null));

        context.AppsAncestors.AddRange(
            new AppAncestor { AppId = tree.ParentId, AncestorId = tree.GrandparentId, Depth = 1 },
            new AppAncestor { AppId = tree.ChildId, AncestorId = tree.ParentId, Depth = 1 },
            new AppAncestor { AppId = tree.ChildId, AncestorId = tree.GrandparentId, Depth = 2 });

        context.SaveChanges();

        return tree;
    }

    internal static AppEfCore NewApp(
        Guid id,
        string name,
        Guid? fatherAppId)
    {
        return new AppEfCore
        {
            Id = id,
            Name = name,
            TenantId = AuthConstants.SEED_TENANT_ID,
            FatherAppId = fatherAppId,
            Logo = new Logo
            {
                ColorKey = $"{name}-color.png",
                LightKey = $"{name}-light.png",
                DarkKey = $"{name}-dark.png",
            },
        };
    }

    internal static RoleEfCore NewRole(
        Guid appId,
        string name,
        bool isPublic)
    {
        return new RoleEfCore
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = name,
            AppId = appId,
            TenantId = AuthConstants.SEED_TENANT_ID,
            IsPublic = isPublic,
        };
    }

    internal static PermissionEfCore NewPermission(
        Guid appId,
        string key,
        bool isPublic)
    {
        return new PermissionEfCore
        {
            Id = Guid.NewGuid(),
            Name = key,
            Description = key,
            Key = key,
            AppId = appId,
            TenantId = AuthConstants.SEED_TENANT_ID,
            IsPublic = isPublic,
        };
    }
}
