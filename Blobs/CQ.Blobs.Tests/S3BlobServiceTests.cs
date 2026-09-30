using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using CQ.Blobs.Aws;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CQ.Blobs.Tests;

[TestClass]
public sealed class S3BlobServiceTests
{
    private Mock<IAmazonS3> _client = null!;
    private Mock<ILogger<S3BlobService>> _logger = null!;

    [TestInitialize]
    public void Setup()
    {
        _client = new Mock<IAmazonS3>(MockBehavior.Strict);
        _logger = new Mock<ILogger<S3BlobService>>();
    }

    private S3BlobService CreateService(BlobOptions? options = null)
        => new(_client.Object, Options.Create(options ?? new BlobOptions { BucketName = "bucket" }), _logger.Object);

    private static AmazonS3Exception NotFound() => new("Not Found") { StatusCode = HttpStatusCode.NotFound };

    [TestMethod]
    public async Task Create_upload_signs_the_content_type_and_the_encryption()
    {
        GetPreSignedUrlRequest? signed = null;
        _client
            .Setup(c => c.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .Callback<GetPreSignedUrlRequest>(r => signed = r)
            .ReturnsAsync("https://s3/write");
        _client
            .Setup(c => c.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns("https://s3/read");

        var upload = await CreateService().CreateUploadAsync("image/png", "CQ");

        Assert.IsNotNull(signed);
        Assert.AreEqual(HttpVerb.PUT, signed.Verb);
        Assert.AreEqual("bucket", signed.BucketName);
        Assert.AreEqual(upload.Key, signed.Key);
        Assert.AreEqual("image/png", signed.ContentType);
        Assert.AreEqual(ServerSideEncryptionMethod.AES256, signed.ServerSideEncryptionMethod);
        Assert.AreEqual("https://s3/write", upload.WriteUrl);
        Assert.AreEqual("https://s3/read", upload.ReadUrl);
    }

    [TestMethod]
    public void Get_by_key_uses_the_cdn_when_configured()
    {
        var service = CreateService(new BlobOptions { CloudFrontDomain = "https://cdn.example.com/" });

        var blob = service.GetByKey("/categories/a.png");

        Assert.AreEqual("https://cdn.example.com/categories/a.png", blob.Url);
    }

    [TestMethod]
    public void Get_by_key_without_cdn_is_presigned_with_the_configured_expiration()
    {
        GetPreSignedUrlRequest? signed = null;
        _client
            .Setup(c => c.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Callback<GetPreSignedUrlRequest>(r => signed = r)
            .Returns("https://s3/read");
        var service = CreateService(new BlobOptions { PresignedUrlExpiration = TimeSpan.FromMinutes(5) });

        var blob = service.GetByKey("categories/a.png");

        Assert.AreEqual("https://s3/read", blob.Url);
        Assert.IsNotNull(signed);
        Assert.AreEqual(HttpVerb.GET, signed.Verb);
        Assert.IsTrue(signed.Expires <= DateTime.UtcNow.AddMinutes(5));
        Assert.IsTrue(signed.Expires > DateTime.UtcNow.AddMinutes(4));
    }

    [TestMethod]
    public void Get_presigned_by_key_applies_the_inline_overrides()
    {
        GetPreSignedUrlRequest? signed = null;
        _client
            .Setup(c => c.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Callback<GetPreSignedUrlRequest>(r => signed = r)
            .Returns("https://s3/read");

        CreateService(new BlobOptions { CloudFrontDomain = "https://cdn" })
            .GetPresignedByKey("orders/1.pdf", new PresignedReadOptions
            {
                InlineFileName = "Orden 1.pdf",
                ContentType = "application/pdf",
            });

        Assert.IsNotNull(signed);
        Assert.AreEqual("application/pdf", signed.ResponseHeaderOverrides.ContentType);
        Assert.AreEqual(
            "inline; filename=\"Orden 1.pdf\"; filename*=UTF-8''Orden%201.pdf",
            signed.ResponseHeaderOverrides.ContentDisposition);
    }

    [TestMethod]
    public void Inline_disposition_keeps_the_ascii_fallback_safe()
    {
        Assert.AreEqual(
            "inline; filename=\"Cotizaci_n _a_.pdf\"; filename*=UTF-8''Cotizaci%C3%B3n%20%22a%22.pdf",
            S3BlobService.InlineDisposition("Cotización \"a\".pdf"));
    }

    [TestMethod]
    public async Task Promote_copies_encrypted_inside_the_bucket()
    {
        CopyObjectRequest? copy = null;
        _client
            .Setup(c => c.CopyObjectAsync(It.IsAny<CopyObjectRequest>(), It.IsAny<CancellationToken>()))
            .Callback<CopyObjectRequest, CancellationToken>((r, _) => copy = r)
            .ReturnsAsync(new CopyObjectResponse());

        var key = await CreateService().PromoteAsync("temporary/cq/app/f.png", "cq/app", "cq");

        Assert.AreEqual("cq/app/f.png", key);
        Assert.IsNotNull(copy);
        Assert.AreEqual("bucket", copy.SourceBucket);
        Assert.AreEqual("temporary/cq/app/f.png", copy.SourceKey);
        Assert.AreEqual("bucket", copy.DestinationBucket);
        Assert.AreEqual("cq/app/f.png", copy.DestinationKey);
        Assert.AreEqual(ServerSideEncryptionMethod.AES256, copy.ServerSideEncryptionMethod);
    }

    [TestMethod]
    public async Task Promote_of_a_temporary_that_s3_no_longer_has_says_it_expired()
    {
        _client
            .Setup(c => c.CopyObjectAsync(It.IsAny<CopyObjectRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(NotFound());

        await Assert.ThrowsExceptionAsync<BlobTemporaryExpiredException>(
            () => CreateService().PromoteAsync("temporary/f.png", "categories"));
    }

    [TestMethod]
    public async Task Promote_does_not_hide_other_s3_errors()
    {
        _client
            .Setup(c => c.CopyObjectAsync(It.IsAny<CopyObjectRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("Forbidden") { StatusCode = HttpStatusCode.Forbidden });

        await Assert.ThrowsExceptionAsync<AmazonS3Exception>(
            () => CreateService().PromoteAsync("temporary/f.png", "categories"));
    }

    [TestMethod]
    public async Task Commit_logs_and_swallows_a_failed_delete()
    {
        _client
            .Setup(c => c.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("boom"));

        await CreateService().CommitAsync(new BlobReplacement("categories/new.png", "categories/old.png"));

        _logger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<AmazonS3Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Commit_deletes_the_previous_key_from_the_bucket()
    {
        DeleteObjectRequest? deleted = null;
        _client
            .Setup(c => c.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()))
            .Callback<DeleteObjectRequest, CancellationToken>((r, _) => deleted = r)
            .ReturnsAsync(new DeleteObjectResponse());

        await CreateService().CommitAsync(new BlobReplacement("categories/new.png", "categories/old.png"));

        Assert.IsNotNull(deleted);
        Assert.AreEqual("bucket", deleted.BucketName);
        Assert.AreEqual("categories/old.png", deleted.Key);
    }

    [TestMethod]
    public async Task Upload_is_encrypted_and_leaves_the_stream_open()
    {
        PutObjectRequest? put = null;
        _client
            .Setup(c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutObjectRequest, CancellationToken>((r, _) => put = r)
            .ReturnsAsync(new PutObjectResponse());

        await CreateService().UploadAsync("banners/a.html", Stream.Null, "text/html");

        Assert.IsNotNull(put);
        Assert.AreEqual("banners/a.html", put.Key);
        Assert.AreEqual("text/html", put.ContentType);
        Assert.IsFalse(put.AutoCloseStream);
        Assert.AreEqual(ServerSideEncryptionMethod.AES256, put.ServerSideEncryptionMethod);
    }

    [TestMethod]
    public async Task Open_read_of_a_missing_key_throws_not_found()
    {
        _client
            .Setup(c => c.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(NotFound());

        var error = await Assert.ThrowsExceptionAsync<BlobNotFoundException>(
            () => CreateService().OpenReadAsync("reports/missing.xlsx"));

        Assert.AreEqual("reports/missing.xlsx", error.Key);
    }

    [TestMethod]
    public async Task Open_read_copies_the_content_to_a_rewound_stream()
    {
        _client
            .Setup(c => c.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new GetObjectResponse { ResponseStream = new MemoryStream([1, 2, 3]) });

        using var stream = await CreateService().OpenReadAsync("reports/a.bin");

        Assert.AreEqual(0, stream.Position);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, stream.ToArray());
    }
}
