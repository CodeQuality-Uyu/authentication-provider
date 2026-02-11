namespace CQ.AuthProvider.BusinessLogic.AppConfig;

public sealed record BlobSection
{
    public BlobType Type { get; init; }

    public string BucketName { get; init; } = "blobs";

    public string TemporaryObject { get; init; } = "temporary";
}

public enum BlobType
{
    Mock,
    LocalStack,
    Aws
}
