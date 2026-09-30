using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CQ.Blobs.AspNetCore.Tests;

[TestClass]
public sealed class BlobControllerBaseTests
{
    private const string Guid = "[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}";

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
        => await response.Content.ReadFromJsonAsync<JsonElement>();

    private static async Task AssertErrorAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.AreEqual(status, response.StatusCode);
        Assert.AreEqual(code, (await ReadJsonAsync(response)).GetProperty("code").GetString());
    }

    #region POST sin key

    [TestMethod]
    public async Task Post_reserves_a_temporary_key_and_returns_the_existing_contract()
    {
        await using var host = await TestHost.StartAsync();

        var response = await host.Client.PostAsJsonAsync("unscoped/blobs", new { contentType = "image/png" });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadJsonAsync(response);
        StringAssert.Matches(body.GetProperty("key").GetString(), new Regex($"^temporary/{Guid}\\.png$"));
        Assert.IsFalse(string.IsNullOrEmpty(body.GetProperty("readUrl").GetString()));
        Assert.IsFalse(string.IsNullOrEmpty(body.GetProperty("writeUrl").GetString()));
    }

    [TestMethod]
    public async Task Post_reserves_under_the_upload_scope()
    {
        await using var host = await TestHost.StartAsync();

        var response = await host.Client.PostAsJsonAsync("scoped/blobs", new { contentType = "image/png", app = "Other App" });

        var body = await ReadJsonAsync(response);
        StringAssert.Matches(
            body.GetProperty("key").GetString(),
            new Regex($"^temporary/tenant-a/other-app/{Guid}\\.png$"));
    }

    #endregion

    #region Content type

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("png")]
    [DataRow("image/")]
    public async Task Post_rejects_a_missing_or_invalid_content_type(string? contentType)
    {
        await using var host = await TestHost.StartAsync();

        var response = await host.Client.PostAsJsonAsync("unscoped/blobs", new { contentType });

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, BlobEndpointErrorCodes.ContentTypeNotAllowed);
    }

    [DataTestMethod]
    [DataRow("image/png", HttpStatusCode.OK)]
    [DataRow("IMAGE/SVG+XML", HttpStatusCode.OK)]
    [DataRow("application/pdf", HttpStatusCode.OK)]
    [DataRow("Application/PDF; charset=binary", HttpStatusCode.OK)]
    [DataRow("application/zip", HttpStatusCode.BadRequest)]
    [DataRow("imagex/png", HttpStatusCode.BadRequest)]
    public async Task Post_applies_the_allowed_content_types(string contentType, HttpStatusCode expected)
    {
        await using var host = await TestHost.StartAsync(new()
        {
            ["Blob:Endpoints:AllowedContentTypes:0"] = "image/*",
            ["Blob:Endpoints:AllowedContentTypes:1"] = "application/pdf",
        });

        var response = await host.Client.PostAsJsonAsync("unscoped/blobs", new { contentType });

        Assert.AreEqual(expected, response.StatusCode);
    }

    #endregion

    #region POST con key

    [TestMethod]
    public async Task Post_with_a_key_is_rejected_by_default()
    {
        await using var host = await TestHost.StartAsync();

        var response = await host.Client.PostAsJsonAsync("unscoped/blobs", new { contentType = "image/png", key = "products/a.png" });

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, BlobEndpointErrorCodes.ClientKeyNotAllowed);
    }

    [TestMethod]
    public async Task Post_with_a_key_and_the_flag_overwrites_any_key_without_scope()
    {
        await using var host = await TestHost.StartAsync(new() { ["Blob:Endpoints:AllowClientKey"] = "true" });

        var response = await host.Client.PostAsJsonAsync("unscoped/blobs", new { contentType = "image/png", key = "products/a.png" });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("products/a.png", (await ReadJsonAsync(response)).GetProperty("key").GetString());
    }

    [DataTestMethod]
    [DataRow("tenant-a/app-x/logo.png")]
    [DataRow("tenant-a/other-app/logo.png")]
    [DataRow("temporary/tenant-a/app-x/pending.png")]
    public async Task Post_with_a_key_accepts_keys_of_the_read_scope(string key)
    {
        await using var host = await TestHost.StartAsync(new() { ["Blob:Endpoints:AllowClientKey"] = "true" });

        var response = await host.Client.PostAsJsonAsync("scoped/blobs", new { contentType = "image/png", key });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(key, (await ReadJsonAsync(response)).GetProperty("key").GetString());
    }

    [DataTestMethod]
    [DataRow("tenant-b/app/logo.png")]
    [DataRow("tenant-a-evil/logo.png")]
    [DataRow("temporary/tenant-b/pending.png")]
    public async Task Post_with_a_key_of_another_scope_is_forbidden(string key)
    {
        await using var host = await TestHost.StartAsync(new() { ["Blob:Endpoints:AllowClientKey"] = "true" });

        var response = await host.Client.PostAsJsonAsync("scoped/blobs", new { contentType = "image/png", key });

        await AssertErrorAsync(response, HttpStatusCode.Forbidden, BlobErrorCodes.KeyOutOfScope);
    }

    [DataTestMethod]
    [DataRow("tenant-a/../tenant-b/logo.png")]
    [DataRow("tenant-a//logo.png")]
    [DataRow("tenant-a/")]
    public async Task Post_with_an_invalid_key_is_rejected_even_with_the_flag(string key)
    {
        await using var host = await TestHost.StartAsync(new() { ["Blob:Endpoints:AllowClientKey"] = "true" });

        var response = await host.Client.PostAsJsonAsync("scoped/blobs", new { contentType = "image/png", key });

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, BlobEndpointErrorCodes.ClientKeyNotAllowed);
    }

    #endregion

    #region GET

    [TestMethod]
    public async Task Get_matches_keys_with_slashes()
    {
        await using var host = await TestHost.StartAsync();

        var response = await host.Client.GetAsync("unscoped/blobs/temporary/any/a.png");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadJsonAsync(response);
        Assert.AreEqual("temporary/any/a.png", body.GetProperty("key").GetString());
        Assert.IsFalse(string.IsNullOrEmpty(body.GetProperty("url").GetString()));
    }

    [DataTestMethod]
    [DataRow("tenant-a/app-x/logo.png")]
    [DataRow("temporary/tenant-a/app-x/pending.png")]
    public async Task Get_returns_keys_of_the_read_scope(string key)
    {
        await using var host = await TestHost.StartAsync();

        var response = await host.Client.GetAsync($"scoped/blobs/{key}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [DataTestMethod]
    [DataRow("tenant-b/app/logo.png")]
    [DataRow("logo.png")]
    [DataRow("temporary/tenant-b/pending.png")]
    public async Task Get_of_another_scope_is_forbidden(string key)
    {
        await using var host = await TestHost.StartAsync();

        var response = await host.Client.GetAsync($"scoped/blobs/{key}");

        await AssertErrorAsync(response, HttpStatusCode.Forbidden, BlobErrorCodes.KeyOutOfScope);
    }

    #endregion
}
