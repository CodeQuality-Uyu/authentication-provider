namespace CQ.Blobs.AspNetCore;

public static class BlobEndpointErrorCodes
{
    public const string ContentTypeNotAllowed = "BLOB_CONTENT_TYPE_NOT_ALLOWED";

    public const string ClientKeyNotAllowed = "BLOB_CLIENT_KEY_NOT_ALLOWED";
}

/// <summary>
/// El content type falta, no tiene formato <c>tipo/subtipo</c> o no está en
/// <see cref="BlobEndpointOptions.AllowedContentTypes"/>.
/// </summary>
public sealed class BlobContentTypeNotAllowedException(string? contentType)
    : BlobException(BlobEndpointErrorCodes.ContentTypeNotAllowed, key: null)
{
    public string? ContentType { get; } = contentType;
}

/// <summary>
/// Se mandó una key a sobrescribir sin <see cref="BlobEndpointOptions.AllowClientKey"/>,
/// o la key no es válida.
/// </summary>
public sealed class BlobClientKeyNotAllowedException(string key)
    : BlobException(BlobEndpointErrorCodes.ClientKeyNotAllowed, key);
