namespace CQ.Blobs;

/// <summary>
/// Sección <c>Blob</c> del appsettings. Los nombres son los que ya usan el Auth Provider y
/// eColors, para que migrar no obligue a tocar la configuración.
/// </summary>
public sealed record BlobOptions
{
    public BlobType Type { get; init; }

    public string BucketName { get; init; } = "blobs";

    /// <summary>
    /// Carpeta donde caen las subidas hasta que el recurso las promueve. El bucket tiene que
    /// tener una lifecycle rule que la limpie: la promoción copia, no borra el temporal.
    /// </summary>
    public string TemporaryObject { get; init; } = "temporary";

    /// <summary>
    /// Con valor, <see cref="IBlobService.GetByKey"/> arma URLs públicas que no vencen
    /// (<c>{CloudFrontDomain}/{key}</c>). Vacío, devuelve URLs prefirmadas.
    /// </summary>
    public string CloudFrontDomain { get; init; } = string.Empty;

    public TimeSpan PresignedUrlExpiration { get; init; } = TimeSpan.FromMinutes(15);
}

public enum BlobType
{
    /// <summary>
    /// En memoria, sin AWS. Para desarrollo y tests.
    /// </summary>
    Mock,

    /// <summary>
    /// S3 emulado con LocalStack. Lee la sección <c>LocalStack</c>.
    /// </summary>
    LocalStack,

    /// <summary>
    /// S3 real. Lee la sección <c>AWS</c> (perfil y región).
    /// </summary>
    Aws,
}

/// <summary>
/// Sección <c>LocalStack</c>.
/// </summary>
public sealed record LocalStackOptions
{
    public string AccessToken { get; init; } = "test";

    public string SecretToken { get; init; } = "test";

    public string ServiceUrl { get; init; } = "http://localhost:4566";
}

/// <summary>
/// Sección <c>FakeBlob</c>: archivos locales que el fake en memoria sirve por key, para
/// probar sin AWS los flujos que leen el contenido (por ejemplo, procesar un Excel).
/// </summary>
public sealed record FakeBlobOptions
{
    public IList<FakeBlobFile> Blobs { get; init; } = [];
}

public sealed record FakeBlobFile
{
    public string Key { get; init; } = null!;

    public string Path { get; init; } = null!;
}
