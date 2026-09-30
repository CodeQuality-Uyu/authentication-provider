using System.Text;
using CQ.Blobs.Fake;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CQ.Blobs.Tests;

/// <summary>
/// Lo que comparten todas las implementaciones (keys, promoción y reemplazo), probado
/// sobre el fake en memoria.
/// </summary>
[TestClass]
public sealed class BlobServiceTests
{
    private InMemoryBlobService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new InMemoryBlobService(
            Options.Create(new BlobOptions()),
            Options.Create(new FakeBlobOptions()),
            NullLogger<InMemoryBlobService>.Instance);
    }

    private async Task<string> UploadTemporaryAsync(params string[] scope)
    {
        var upload = await _service.CreateUploadAsync("image/png", scope);

        return upload.Key;
    }

    #region CreateUploadAsync

    [TestMethod]
    public async Task CreateUpload_reserves_a_key_in_the_temporary_folder_under_the_scope()
    {
        var upload = await _service.CreateUploadAsync("image/png", "Creative Color Labs", "Admin App");

        StringAssert.Matches(
            upload.Key,
            new System.Text.RegularExpressions.Regex(@"^temporary/creative-color-labs/admin-app/[0-9a-f-]{36}\.png$"));
        Assert.IsTrue(_service.IsTemporaryKey(upload.Key));
        Assert.IsTrue(_service.Exists(upload.Key));
    }

    [TestMethod]
    public async Task CreateUpload_without_scope_goes_to_the_temporary_root()
    {
        var upload = await _service.CreateUploadAsync("image/svg+xml");

        StringAssert.Matches(
            upload.Key,
            new System.Text.RegularExpressions.Regex(@"^temporary/[0-9a-f-]{36}\.svg$"));
    }

    #endregion

    #region PromoteAsync

    [TestMethod]
    public async Task Promote_copies_to_the_destination_keeping_the_file_name()
    {
        var temporaryKey = await UploadTemporaryAsync("CQ");

        var key = await _service.PromoteAsync(temporaryKey, "CQ/Admin App");

        Assert.AreEqual($"cq/admin-app/{BlobKey.FileName(temporaryKey)}", key);
        Assert.IsTrue(_service.Exists(key));
        Assert.IsTrue(_service.Exists(temporaryKey), "The temporary is left for the lifecycle rule.");
    }

    [TestMethod]
    public async Task Promote_accepts_a_leading_slash()
    {
        var temporaryKey = await UploadTemporaryAsync();

        var key = await _service.PromoteAsync($"/{temporaryKey}", "categories");

        Assert.AreEqual($"categories/{BlobKey.FileName(temporaryKey)}", key);
    }

    [DataTestMethod]
    [DataRow("categories/x.png")]
    [DataRow("x.png")]
    [DataRow("temporary")]
    [DataRow("temporary/")]
    [DataRow("temporary/cq/")]
    [DataRow("temporaryx/x.png")]
    [DataRow("other/temporary/x.png")]
    [DataRow("temporary/../categories/x.png")]
    [DataRow("temporary/./x.png")]
    public async Task Promote_rejects_a_key_that_is_not_a_temporary_file(string key)
    {
        await Assert.ThrowsExceptionAsync<BlobKeyNotInTemporaryException>(
            () => _service.PromoteAsync(key, "categories"));
    }

    [TestMethod]
    public async Task Promote_with_scope_accepts_a_key_of_that_scope()
    {
        var temporaryKey = await UploadTemporaryAsync("CQ", "Admin");

        var key = await _service.PromoteAsync(temporaryKey, "cq/admin", "CQ");

        Assert.IsTrue(_service.Exists(key));
    }

    [TestMethod]
    public async Task Promote_with_scope_rejects_a_key_of_another_scope()
    {
        var temporaryKey = await UploadTemporaryAsync("Pinturas");

        await Assert.ThrowsExceptionAsync<BlobKeyOutOfScopeException>(
            () => _service.PromoteAsync(temporaryKey, "cq", "CQ"));
    }

    [TestMethod]
    public async Task Promote_with_scope_does_not_match_a_scope_that_only_shares_the_prefix()
    {
        var temporaryKey = await UploadTemporaryAsync("cq-evil");

        await Assert.ThrowsExceptionAsync<BlobKeyOutOfScopeException>(
            () => _service.PromoteAsync(temporaryKey, "cq", "cq"));
    }

    [TestMethod]
    public async Task Promote_with_scope_rejects_an_unscoped_key()
    {
        var temporaryKey = await UploadTemporaryAsync();

        await Assert.ThrowsExceptionAsync<BlobKeyOutOfScopeException>(
            () => _service.PromoteAsync(temporaryKey, "cq", "cq"));
    }

    [TestMethod]
    public async Task Promote_of_a_missing_temporary_says_it_expired()
    {
        var error = await Assert.ThrowsExceptionAsync<BlobTemporaryExpiredException>(
            () => _service.PromoteAsync("temporary/gone.png", "categories"));

        Assert.AreEqual(BlobErrorCodes.TemporaryExpired, error.Message);
        Assert.AreEqual("temporary/gone.png", error.Key);
    }

    [TestMethod]
    public void Errors_are_invalid_operations_whose_message_is_the_code()
    {
        InvalidOperationException error = new BlobKeyNotInTemporaryException("x.png");

        Assert.AreEqual(BlobErrorCodes.KeyNotInTemporary, error.Message);
    }

    #endregion

    #region IsInScope

    [DataTestMethod]
    [DataRow("tenant-a/app/x.png", true)]
    [DataRow("/tenant-a/x.png", true)]
    [DataRow("temporary/tenant-a/app/x.png", true)]
    [DataRow("tenant-b/x.png", false)]
    [DataRow("tenant-a-evil/x.png", false)]
    [DataRow("temporary/tenant-b/x.png", false)]
    [DataRow("temporary/x.png", false)]
    [DataRow("tenant-a", false)]
    [DataRow("tenant-a/../tenant-b/x.png", false)]
    [DataRow("other/tenant-a/x.png", false)]
    public void IsInScope_checks_final_and_temporary_keys_of_the_scope(string key, bool expected)
    {
        Assert.AreEqual(expected, _service.IsInScope(key, "Tenant A"));
    }

    [TestMethod]
    public void IsInScope_without_scope_accepts_any_key()
    {
        Assert.IsTrue(_service.IsInScope("anything/x.png"));
    }

    #endregion

    #region Reemplazo

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    [DataRow("categories/current.png")]
    public async Task Stage_without_a_new_key_keeps_the_current_one(string? incomingKey)
    {
        var replacement = await _service.StageReplacementAsync(incomingKey, "categories/current.png", "categories");

        Assert.AreEqual("categories/current.png", replacement.Key);
        Assert.IsFalse(replacement.HasChanged);
        Assert.AreEqual(0, _service.Keys.Count, "Nothing is touched.");
    }

    [TestMethod]
    public async Task Stage_with_a_new_key_promotes_it()
    {
        var temporaryKey = await UploadTemporaryAsync();

        var replacement = await _service.StageReplacementAsync(temporaryKey, "categories/current.png", "categories");

        Assert.AreEqual($"categories/{BlobKey.FileName(temporaryKey)}", replacement.Key);
        Assert.AreEqual("categories/current.png", replacement.PreviousKey);
        Assert.IsTrue(replacement.HasChanged);
        Assert.IsTrue(_service.Exists(replacement.Key!));
    }

    [TestMethod]
    public async Task Stage_checks_the_scope_like_promote()
    {
        var temporaryKey = await UploadTemporaryAsync("Pinturas");

        await Assert.ThrowsExceptionAsync<BlobKeyOutOfScopeException>(
            () => _service.StageReplacementAsync(temporaryKey, null, "cq", "CQ"));
    }

    [TestMethod]
    public async Task Commit_deletes_the_previous_blob_and_keeps_the_new_one()
    {
        await _service.UploadAsync("categories/current.png", Stream.Null, "image/png");
        var replacement = await _service.StageReplacementAsync(
            await UploadTemporaryAsync(),
            "categories/current.png",
            "categories");

        await _service.CommitAsync(replacement);

        Assert.IsFalse(_service.Exists("categories/current.png"));
        Assert.IsTrue(_service.Exists(replacement.Key!));
    }

    [TestMethod]
    public async Task Rollback_deletes_the_new_blob_and_keeps_the_previous_one()
    {
        await _service.UploadAsync("categories/current.png", Stream.Null, "image/png");
        var replacement = await _service.StageReplacementAsync(
            await UploadTemporaryAsync(),
            "categories/current.png",
            "categories");

        await _service.RollbackAsync(replacement);

        Assert.IsTrue(_service.Exists("categories/current.png"));
        Assert.IsFalse(_service.Exists(replacement.Key!));
    }

    [TestMethod]
    public async Task Commit_and_rollback_of_an_unchanged_replacement_do_nothing()
    {
        await _service.UploadAsync("categories/current.png", Stream.Null, "image/png");
        var replacement = BlobReplacement.Unchanged("categories/current.png");

        await _service.CommitAsync(replacement);
        await _service.RollbackAsync(replacement);

        Assert.IsTrue(_service.Exists("categories/current.png"));
    }

    [TestMethod]
    public async Task Commit_of_a_removal_deletes_the_previous_blob()
    {
        await _service.UploadAsync("categories/current.png", Stream.Null, "image/png");
        var replacement = BlobReplacement.Removal("categories/current.png");

        await _service.CommitAsync(replacement);

        Assert.IsNull(replacement.Key);
        Assert.IsFalse(_service.Exists("categories/current.png"));
    }

    [TestMethod]
    public async Task Commit_of_a_first_blob_has_nothing_to_delete()
    {
        var replacement = await _service.StageReplacementAsync(await UploadTemporaryAsync(), null, "categories");

        await _service.CommitAsync(replacement);

        Assert.IsTrue(_service.Exists(replacement.Key!));
    }

    #endregion

    #region Contenido

    [TestMethod]
    public async Task Upload_and_open_read_round_trip_the_content_without_closing_the_stream()
    {
        using var content = new MemoryStream(Encoding.UTF8.GetBytes("hola"));

        await _service.UploadAsync("reports/a.txt", content, "text/plain");
        using var read = await _service.OpenReadAsync("reports/a.txt");

        Assert.IsTrue(content.CanRead, "The caller's stream stays open.");
        Assert.AreEqual("hola", Encoding.UTF8.GetString(read.ToArray()));
    }

    [TestMethod]
    public async Task Open_read_of_a_missing_key_throws_not_found()
    {
        await Assert.ThrowsExceptionAsync<BlobNotFoundException>(
            () => _service.OpenReadAsync("reports/missing.txt"));
    }

    [TestMethod]
    public async Task Open_read_falls_back_to_the_configured_local_files()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "excel");
        try
        {
            var service = new InMemoryBlobService(
                Options.Create(new BlobOptions()),
                Options.Create(new FakeBlobOptions { Blobs = [new FakeBlobFile { Key = "colors.xlsx", Path = path }] }),
                NullLogger<InMemoryBlobService>.Instance);

            using var read = await service.OpenReadAsync("colors.xlsx");

            Assert.AreEqual("excel", Encoding.UTF8.GetString(read.ToArray()));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Get_by_key_returns_a_url_key_as_is_and_a_placeholder_otherwise()
    {
        Assert.AreEqual("https://img.example.com/a.png", _service.GetByKey("https://img.example.com/a.png").Url);
        Assert.AreEqual(InMemoryBlobService.PlaceholderUrl, _service.GetByKey("categories/a.png").Url);
    }

    #endregion
}
