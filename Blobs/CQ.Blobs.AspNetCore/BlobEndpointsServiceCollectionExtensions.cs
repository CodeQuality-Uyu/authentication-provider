using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.Blobs.AspNetCore;

public static class BlobEndpointsServiceCollectionExtensions
{
    /// <summary>
    /// Lee <c>Blob:Endpoints</c>. Va junto con <c>AddBlobs</c>, que registra
    /// <see cref="IBlobService"/>:
    /// <code>services.AddBlobs(configuration).AddBlobEndpoints(configuration);</code>
    /// </summary>
    public static IServiceCollection AddBlobEndpoints(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<BlobEndpointOptions>()
            .Bind(configuration.GetSection(BlobEndpointOptions.Section));

        return services;
    }
}
