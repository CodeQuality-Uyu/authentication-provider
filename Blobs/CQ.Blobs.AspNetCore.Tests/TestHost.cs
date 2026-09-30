using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CQ.Blobs.AspNetCore.Tests;

/// <summary>
/// API sin alcance: un bucket por cliente, como eColors.
/// </summary>
[Route("unscoped/blobs")]
public sealed class UnscopedBlobController : BlobControllerBase<CreateBlobUploadRequest>;

public sealed record ScopedBlobRequest : CreateBlobUploadRequest
{
    public string? App { get; init; }
}

/// <summary>
/// API con alcance, como el Auth Provider: sube bajo el tenant y la app, lee todo el tenant.
/// </summary>
[Route("scoped/blobs")]
public sealed class ScopedBlobController : BlobControllerBase<ScopedBlobRequest>
{
    protected override Task<string[]> ResolveUploadScopeAsync(ScopedBlobRequest request)
        => Task.FromResult(new[] { "Tenant A", request.App ?? "App X" });

    protected override Task<string[]> ResolveReadScopeAsync()
        => Task.FromResult(new[] { "Tenant A" });
}

/// <summary>
/// Lo que haría cada API con <see cref="BlobHttpErrors"/> en su propio manejo de errores.
/// </summary>
public sealed class BlobErrorFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var error = BlobHttpErrors.For(context.Exception);
        if (error is null)
        {
            return;
        }

        context.Result = new ObjectResult(new { code = error.Code }) { StatusCode = (int)error.StatusCode };
        context.ExceptionHandled = true;
    }
}

internal sealed class TestHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    private TestHost(WebApplication app)
    {
        _app = app;
        Client = app.GetTestClient();
    }

    public HttpClient Client { get; }

    public static async Task<TestHost> StartAsync(Dictionary<string, string?>? settings = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Blob:Type"] = "mock" });
        builder.Configuration.AddInMemoryCollection(settings ?? []);

        builder.Services
            .AddControllers(options => options.Filters.Add<BlobErrorFilter>())
            .AddApplicationPart(typeof(TestHost).Assembly);
        builder.Services
            .AddBlobs(builder.Configuration)
            .AddBlobEndpoints(builder.Configuration);

        var app = builder.Build();
        app.MapControllers();
        await app.StartAsync();

        return new TestHost(app);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
    }
}
