using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using CQ.Blobs.Aws;
using CQ.Blobs.Fake;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CQ.Blobs;

public static class BlobServiceCollectionExtensions
{
    public const string BlobSection = "Blob";

    public const string FakeBlobSection = "FakeBlob";

    public const string LocalStackSection = "LocalStack";

    public const string AwsSection = "AWS";

    /// <summary>
    /// Registra <see cref="IBlobService"/> según <c>Blob:Type</c> (<c>mock</c>,
    /// <c>localstack</c> o <c>aws</c>). Usa las mismas secciones que ya tienen el Auth
    /// Provider y eColors: <c>Blob</c>, <c>FakeBlob</c>, <c>LocalStack</c> y <c>AWS</c>.
    /// </summary>
    /// <remarks>
    /// Si la aplicación ya registró un <see cref="IAmazonS3"/>, se reutiliza.
    /// </remarks>
    public static IServiceCollection AddBlobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var blobSection = configuration.GetSection(BlobSection);
        var blobOptions = blobSection.Get<BlobOptions>() ?? new BlobOptions();

        services.AddOptions<BlobOptions>().Bind(blobSection);
        services.AddLogging();

        switch (blobOptions.Type)
        {
            case BlobType.Mock:
                services.AddOptions<FakeBlobOptions>().Bind(configuration.GetSection(FakeBlobSection));
                services.TryAddSingleton<IBlobService, InMemoryBlobService>();
                break;

            case BlobType.LocalStack:
                var localStack = configuration.GetSection(LocalStackSection).Get<LocalStackOptions>()
                    ?? new LocalStackOptions();

                services.TryAddSingleton<IAmazonS3>(_ => new AmazonS3Client(
                    new BasicAWSCredentials(localStack.AccessToken, localStack.SecretToken),
                    new AmazonS3Config
                    {
                        ServiceURL = localStack.ServiceUrl,
                        ForcePathStyle = true,
                    }));
                services.TryAddSingleton<IBlobService, S3BlobService>();
                break;

            case BlobType.Aws:
                services.TryAddSingleton<AWSOptions>(configuration.GetAWSOptions(AwsSection));
                services.TryAddAWSService<IAmazonS3>();
                services.TryAddSingleton<IBlobService, S3BlobService>();
                break;

            default:
                throw new InvalidOperationException($"Invalid blob type: {blobOptions.Type}");
        }

        return services;
    }
}
