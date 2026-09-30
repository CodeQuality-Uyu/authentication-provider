namespace CQ.Blobs;

/// <summary>
/// Códigos de los errores de blobs. Los dos primeros son los que eColors ya tiene en su
/// catálogo, así que se mapean sin cambios.
/// </summary>
public static class BlobErrorCodes
{
    public const string KeyNotInTemporary = "BLOB_KEY_NOT_IN_TEMPORARY";

    public const string TemporaryExpired = "BLOB_TEMPORARY_EXPIRED";

    public const string KeyOutOfScope = "BLOB_KEY_OUT_OF_SCOPE";

    public const string NotFound = "BLOB_NOT_FOUND";
}

/// <summary>
/// Error de negocio de blobs. El mensaje es el código, igual que las
/// <see cref="InvalidOperationException"/> que cada API ya traduce a un status con su
/// catálogo de errores.
/// </summary>
public abstract class BlobException(string code, string? key, Exception? innerException = null)
    : InvalidOperationException(code, innerException)
{
    public string Code { get; } = code;

    /// <summary>
    /// La key involucrada, cuando el error es sobre una.
    /// </summary>
    public string? Key { get; } = key;
}

/// <summary>
/// Se quiso promover una key que no está en la carpeta temporal.
/// </summary>
public sealed class BlobKeyNotInTemporaryException(string key)
    : BlobException(BlobErrorCodes.KeyNotInTemporary, key);

/// <summary>
/// La key es temporal pero de otro alcance (otro tenant, otra app): alguien mandó la key
/// de una subida que no hizo.
/// </summary>
public sealed class BlobKeyOutOfScopeException(string key)
    : BlobException(BlobErrorCodes.KeyOutOfScope, key);

/// <summary>
/// El temporal ya no existe: lo borró la lifecycle rule, o nunca se subió.
/// </summary>
public sealed class BlobTemporaryExpiredException(string key, Exception? innerException = null)
    : BlobException(BlobErrorCodes.TemporaryExpired, key, innerException);

/// <summary>
/// El objeto no existe.
/// </summary>
public sealed class BlobNotFoundException(string key, Exception? innerException = null)
    : BlobException(BlobErrorCodes.NotFound, key, innerException);
