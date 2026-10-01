using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.AuthProvider.DataAccess.EfCore.Apps;
using CQ.AuthProvider.Tests.Infrastructure;
using CQ.Blobs;
using Microsoft.EntityFrameworkCore;

namespace CQ.AuthProvider.Tests;

/// <summary>
/// El reemplazo de logos de <c>PATCH /apps/{id}/logo</c> (<see cref="IAppService.UpdateLogoByIdAsync"/>).
/// </summary>
/// <remarks>
/// Lo que más vale probar es qué se borra: una app cliente creada sin logo propio guarda las
/// mismas keys que su padre, y borrar la key "anterior" de una le rompería el logo a la otra.
/// </remarks>
public sealed class AppLogoUpdateTests
{
    [Fact]
    public async Task Replacing_one_logo_persists_it_and_deletes_only_the_previous_one()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);
        var blobs = new FakeBlobService();

        await NewService(fixture, blobs).UpdateLogoByIdAsync(
            tree.ParentId,
            new UpdateAppLogoArgs(null, "temporary/seed/new-light.png", null),
            Logged(tree.ParentId));

        var logo = await LogoOfAsync(fixture, tree.ParentId);
        Assert.Equal("Parent-color.png", logo.ColorKey);
        Assert.Equal("seed/parent/new-light.png", logo.LightKey);
        Assert.Equal("Parent-dark.png", logo.DarkKey);
        Assert.Equal(["Parent-light.png"], blobs.Deleted);
        Assert.Empty(blobs.RolledBack);
    }

    [Fact]
    public async Task A_key_equal_to_the_current_one_leaves_that_logo_untouched()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);
        var blobs = new FakeBlobService();

        await NewService(fixture, blobs).UpdateLogoByIdAsync(
            tree.ParentId,
            new UpdateAppLogoArgs("Parent-color.png", "", "temporary/seed/new-dark.png"),
            Logged(tree.ParentId));

        var logo = await LogoOfAsync(fixture, tree.ParentId);
        Assert.Equal("Parent-color.png", logo.ColorKey);
        Assert.Equal("Parent-light.png", logo.LightKey);
        Assert.Equal("seed/parent/new-dark.png", logo.DarkKey);
        Assert.Equal(["temporary/seed/new-dark.png"], blobs.Promoted);
        Assert.Equal(["Parent-dark.png"], blobs.Deleted);
    }

    [Fact]
    public async Task A_previous_logo_that_another_app_still_uses_is_not_deleted()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);
        await ShareLogoAsync(fixture, from: tree.ParentId, to: tree.ChildId);
        var blobs = new FakeBlobService();

        // La hija pasa a tener logo propio: el del padre no se toca.
        await NewService(fixture, blobs).UpdateLogoByIdAsync(
            tree.ChildId,
            new UpdateAppLogoArgs("temporary/seed/child-color.png", null, null),
            Logged(tree.ChildId));

        Assert.Equal("seed/child/child-color.png", (await LogoOfAsync(fixture, tree.ChildId)).ColorKey);
        Assert.Equal("Parent-color.png", (await LogoOfAsync(fixture, tree.ParentId)).ColorKey);
        Assert.Empty(blobs.Deleted);
    }

    [Fact]
    public async Task Replacing_the_father_logo_keeps_the_one_its_children_inherited()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);
        await ShareLogoAsync(fixture, from: tree.ParentId, to: tree.ChildId);
        var blobs = new FakeBlobService();

        await NewService(fixture, blobs).UpdateLogoByIdAsync(
            tree.ParentId,
            new UpdateAppLogoArgs("temporary/seed/new-color.png", null, null),
            Logged(tree.ParentId));

        Assert.Equal("Parent-color.png", (await LogoOfAsync(fixture, tree.ChildId)).ColorKey);
        Assert.Empty(blobs.Deleted);
    }

    [Fact]
    public async Task A_failure_midway_rolls_back_what_was_promoted_and_leaves_the_app_as_it_was()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);
        var blobs = new FakeBlobService { FailOn = "temporary/seed/expired.png" };

        await Assert.ThrowsAsync<BlobTemporaryExpiredException>(() =>
            NewService(fixture, blobs).UpdateLogoByIdAsync(
                tree.ParentId,
                new UpdateAppLogoArgs("temporary/seed/new-color.png", null, "temporary/seed/expired.png"),
                Logged(tree.ParentId)));

        var logo = await LogoOfAsync(fixture, tree.ParentId);
        Assert.Equal("Parent-color.png", logo.ColorKey);
        Assert.Equal("Parent-dark.png", logo.DarkKey);
        Assert.Equal(["seed/parent/new-color.png"], blobs.RolledBack);
        Assert.Empty(blobs.Deleted);
    }

    [Fact]
    public async Task An_app_of_another_tenant_is_rejected_before_touching_storage()
    {
        using var fixture = new AuthDbContextFixture();
        var tree = TestTree.CreateIn(fixture.Context);
        var blobs = new FakeBlobService();

        var logged = Logged(tree.ParentId) with { Tenant = new Tenant { Id = Guid.NewGuid(), Name = "Other" } };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            NewService(fixture, blobs).UpdateLogoByIdAsync(
                tree.ParentId,
                new UpdateAppLogoArgs("temporary/other/new-color.png", null, null),
                logged));

        Assert.Empty(blobs.Promoted);
        Assert.Equal("Parent-color.png", (await LogoOfAsync(fixture, tree.ParentId)).ColorKey);
    }

    private static async Task ShareLogoAsync(
        AuthDbContextFixture fixture,
        Guid from,
        Guid to)
    {
        var shared = await LogoOfAsync(fixture, from);

        await fixture.Context.Apps
            .Where(a => a.Id == to)
            .ExecuteUpdateAsync(setter => setter.SetProperty(a => a.Logo, shared));

        // ExecuteUpdate no toca las entidades rastreadas: sin esto, el servicio leería el logo
        // que la app tenía al sembrarse (en producción cada request usa un contexto nuevo).
        fixture.Context.ChangeTracker.Clear();
    }

    private static async Task<Logo> LogoOfAsync(
        AuthDbContextFixture fixture,
        Guid appId)
    {
        // Sin tracking: ExecuteUpdate no actualiza las entidades que ya están en el contexto.
        var app = await fixture.Context.Apps
            .AsNoTracking()
            .SingleAsync(a => a.Id == appId);

        return app.Logo;
    }

    private static AccountLogged Logged(Guid appId)
    {
        return new AccountLogged
        {
            Tenant = new Tenant { Id = AuthConstants.SEED_TENANT_ID, Name = "Seed" },
            Apps = [new App { Id = appId }],
            Roles = [],
        };
    }

    private static IAppService NewService(
        AuthDbContextFixture fixture,
        IBlobService blobService)
    {
        var repository = new AppRepository(fixture.Context, DataAccessMapper.Create());

        // Las demás dependencias no se usan en el reemplazo de logos.
        return new AppService(repository, null!, null!, blobService, null!);
    }

    /// <summary>
    /// Registra lo que se promueve, se borra y se revierte. La key promovida es
    /// <c>{carpeta}/{archivo}</c>, como la real.
    /// </summary>
    private sealed class FakeBlobService
        : IBlobService
    {
        public string? FailOn { get; init; }

        public List<string> Promoted { get; } = [];

        public List<string> Deleted { get; } = [];

        public List<string> RolledBack { get; } = [];

        public Task<BlobReplacement> StageReplacementAsync(
            string? incomingKey,
            string? currentKey,
            string destinationFolder,
            params string[] requiredScope)
        {
            if (string.IsNullOrWhiteSpace(incomingKey) || incomingKey == currentKey)
            {
                return Task.FromResult(BlobReplacement.Unchanged(currentKey));
            }

            if (incomingKey == FailOn)
            {
                throw new BlobTemporaryExpiredException(incomingKey);
            }

            Promoted.Add(incomingKey);

            return Task.FromResult(new BlobReplacement(
                $"{destinationFolder}/{incomingKey.Split('/')[^1]}",
                currentKey));
        }

        public Task CommitAsync(BlobReplacement replacement)
        {
            if (replacement.HasChanged && replacement.PreviousKey is not null)
            {
                Deleted.Add(replacement.PreviousKey);
            }

            return Task.CompletedTask;
        }

        public Task RollbackAsync(BlobReplacement replacement)
        {
            if (replacement.HasChanged && replacement.Key is not null)
            {
                RolledBack.Add(replacement.Key);
            }

            return Task.CompletedTask;
        }

        public Task<BlobReadWrite> CreateUploadAsync(string contentType, params string[] scope) => throw new NotSupportedException();

        public Task<BlobReadWrite> CreateUploadForKeyAsync(string key, string contentType) => throw new NotSupportedException();

        public BlobRead GetByKey(string key) => throw new NotSupportedException();

        public BlobRead GetPresignedByKey(string key, PresignedReadOptions? options = null) => throw new NotSupportedException();

        public bool IsTemporaryKey(string key) => throw new NotSupportedException();

        public bool IsInScope(string key, params string[] scope) => throw new NotSupportedException();

        public Task<string> PromoteAsync(string temporaryKey, string destinationFolder, params string[] requiredScope) => throw new NotSupportedException();

        public Task UploadAsync(string key, Stream content, string contentType) => throw new NotSupportedException();

        public Task<MemoryStream> OpenReadAsync(string key) => throw new NotSupportedException();

        public Task DeleteAsync(string key) => throw new NotSupportedException();
    }
}
