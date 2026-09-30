using Amazon.S3;
using CQ.Blobs.Aws;
using CQ.Blobs.Fake;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace CQ.Blobs.Tests;

[TestClass]
public sealed class BlobServiceCollectionExtensionsTests
{
    private static ServiceProvider Build(Dictionary<string, string?> settings, Action<IServiceCollection>? before = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        before?.Invoke(services);
        services.AddBlobs(configuration);

        return services.BuildServiceProvider();
    }

    [TestMethod]
    public void Mock_registers_a_single_in_memory_service()
    {
        using var provider = Build(new() { ["Blob:Type"] = "mock" });

        var first = provider.GetRequiredService<IBlobService>();

        Assert.IsInstanceOfType<InMemoryBlobService>(first);
        Assert.AreSame(first, provider.GetRequiredService<IBlobService>(), "Uploads have to survive between requests.");
    }

    [TestMethod]
    public void Binds_the_blob_section_with_the_existing_setting_names()
    {
        using var provider = Build(new()
        {
            ["Blob:Type"] = "Mock",
            ["Blob:BucketName"] = "ecolors",
            ["Blob:TemporaryObject"] = "tmp",
            ["Blob:CloudFrontDomain"] = "https://cdn",
            ["Blob:PresignedUrlExpiration"] = "00:05:00",
        });

        var options = provider.GetRequiredService<IOptions<BlobOptions>>().Value;

        Assert.AreEqual("ecolors", options.BucketName);
        Assert.AreEqual("tmp", options.TemporaryObject);
        Assert.AreEqual("https://cdn", options.CloudFrontDomain);
        Assert.AreEqual(TimeSpan.FromMinutes(5), options.PresignedUrlExpiration);
    }

    [TestMethod]
    public async Task Mock_uses_the_configured_temporary_folder()
    {
        using var provider = Build(new() { ["Blob:Type"] = "mock", ["Blob:TemporaryObject"] = "tmp" });
        var service = provider.GetRequiredService<IBlobService>();

        var upload = await service.CreateUploadAsync("image/png");

        StringAssert.StartsWith(upload.Key, "tmp/");
    }

    [TestMethod]
    public void Local_stack_registers_s3_against_the_configured_endpoint()
    {
        using var provider = Build(new()
        {
            ["Blob:Type"] = "localstack",
            ["LocalStack:ServiceUrl"] = "http://localhost:4566",
        });

        Assert.IsInstanceOfType<S3BlobService>(provider.GetRequiredService<IBlobService>());

        var client = provider.GetRequiredService<IAmazonS3>();
        StringAssert.StartsWith(client.Config.ServiceURL, "http://localhost:4566");
    }

    [TestMethod]
    public void Reuses_an_s3_client_the_application_already_registered()
    {
        var existing = new Mock<IAmazonS3>().Object;
        using var provider = Build(
            new() { ["Blob:Type"] = "localstack" },
            services => services.AddSingleton(existing));

        Assert.AreSame(existing, provider.GetRequiredService<IAmazonS3>());
    }

    [TestMethod]
    public void Without_a_type_it_defaults_to_mock()
    {
        using var provider = Build([]);

        Assert.IsInstanceOfType<InMemoryBlobService>(provider.GetRequiredService<IBlobService>());
    }
}
