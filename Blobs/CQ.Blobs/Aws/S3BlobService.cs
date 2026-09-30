using System.Net;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CQ.Blobs.Aws;

/// <summary>
/// Blobs sobre S3, real o de LocalStack. Todo lo que escribe va cifrado con AES256.
/// </summary>
public sealed class S3BlobService(
    IAmazonS3 client,
    IOptions<BlobOptions> options,
    ILogger<S3BlobService> logger)
    : BlobServiceBase(options.Value, logger)
{
    public override async Task<BlobReadWrite> CreateUploadForKeyAsync(string key, string contentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        // El cifrado y el content type quedan firmados: el PUT tiene que mandar los mismos
        // headers, o S3 responde 403.
        var request = new GetPreSignedUrlRequest
        {
            BucketName = Options.BucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = ExpiresAt(),
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
        };

        var writeUrl = await client
            .GetPreSignedURLAsync(request)
            .ConfigureAwait(false);

        return new BlobReadWrite(
            key,
            GetByKey(key).Url,
            writeUrl);
    }

    public override BlobRead GetByKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (string.IsNullOrWhiteSpace(Options.CloudFrontDomain))
        {
            return GetPresignedByKey(key);
        }

        return new BlobRead
        {
            Key = key,
            Url = $"{Options.CloudFrontDomain.TrimEnd('/')}/{key.TrimStart('/')}",
        };
    }

    public override BlobRead GetPresignedByKey(string key, PresignedReadOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = Options.BucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = ExpiresAt(),
        };

        if (!string.IsNullOrWhiteSpace(options?.ContentType))
        {
            request.ResponseHeaderOverrides.ContentType = options.ContentType;
        }

        if (!string.IsNullOrWhiteSpace(options?.InlineFileName))
        {
            request.ResponseHeaderOverrides.ContentDisposition = InlineDisposition(options.InlineFileName);
        }

        return new BlobRead
        {
            Key = key,
            Url = client.GetPreSignedURL(request),
        };
    }

    /// <summary>
    /// <c>filename</c> en ASCII para los navegadores viejos y <c>filename*</c> (RFC 5987)
    /// con el nombre real, para que las tildes no rompan el header.
    /// </summary>
    internal static string InlineDisposition(string fileName)
    {
        var ascii = new StringBuilder(fileName.Length);
        foreach (var c in fileName)
        {
            ascii.Append(c is >= ' ' and <= '~' and not '"' and not '\\' ? c : '_');
        }

        return $"inline; filename=\"{ascii}\"; filename*=UTF-8''{Uri.EscapeDataString(fileName)}";
    }

    protected override async Task CopyAsync(string sourceKey, string destinationKey)
    {
        var request = new CopyObjectRequest
        {
            SourceBucket = Options.BucketName,
            SourceKey = sourceKey,
            DestinationBucket = Options.BucketName,
            DestinationKey = destinationKey,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
        };

        try
        {
            await client
                .CopyObjectAsync(request)
                .ConfigureAwait(false);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new BlobNotFoundException(sourceKey, ex);
        }
    }

    public override async Task UploadAsync(string key, Stream content, string contentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        await client
            .PutObjectAsync(new PutObjectRequest
            {
                BucketName = Options.BucketName,
                Key = key,
                InputStream = content,
                ContentType = contentType,

                // El stream es de quien llama: el SDK lo cierra por defecto.
                AutoCloseStream = false,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
            })
            .ConfigureAwait(false);
    }

    public override async Task<MemoryStream> OpenReadAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        GetObjectResponse response;
        try
        {
            response = await client
                .GetObjectAsync(new GetObjectRequest
                {
                    BucketName = Options.BucketName,
                    Key = key,
                })
                .ConfigureAwait(false);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new BlobNotFoundException(key, ex);
        }

        using (response)
        {
            // Se copia a memoria para que el stream siga siendo válido después de liberar
            // la respuesta de S3.
            var stream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(stream).ConfigureAwait(false);
            stream.Position = 0;

            return stream;
        }
    }

    public override async Task DeleteAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        await client
            .DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = Options.BucketName,
                Key = key,
            })
            .ConfigureAwait(false);
    }

    private DateTime ExpiresAt() => DateTime.UtcNow.Add(Options.PresignedUrlExpiration);
}
