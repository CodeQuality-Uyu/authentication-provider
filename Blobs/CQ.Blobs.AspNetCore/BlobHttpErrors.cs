using System.Net;

namespace CQ.Blobs.AspNetCore;

/// <summary>
/// Un error de blobs con su status HTTP sugerido.
/// </summary>
public sealed record BlobHttpError(
    Type ExceptionType,
    string Code,
    HttpStatusCode StatusCode,
    string Message);

/// <summary>
/// Catálogo de los errores de blobs, para registrarlos en el manejo de errores de cada API.
/// </summary>
/// <remarks>
/// El paquete no trae un filtro de excepciones: cada API tiene su propio formato de error
/// (el registro de <c>CQ.ApiElements</c> en el Auth Provider, <c>ErrorCatalog</c> en
/// eColors), y un filtro propio respondería con otro formato. Estas excepciones también
/// salen de los endpoints de cada recurso (al promover una key), no sólo de
/// <see cref="BlobControllerBase{TRequest}"/>.
/// </remarks>
public static class BlobHttpErrors
{
    public static IReadOnlyList<BlobHttpError> All { get; } =
    [
        new(
            typeof(BlobKeyNotInTemporaryException),
            BlobErrorCodes.KeyNotInTemporary,
            HttpStatusCode.BadRequest,
            "The key does not belong to the temporary folder"),
        new(
            typeof(BlobKeyOutOfScopeException),
            BlobErrorCodes.KeyOutOfScope,
            HttpStatusCode.Forbidden,
            "The key does not belong to the caller"),
        new(
            typeof(BlobTemporaryExpiredException),
            BlobErrorCodes.TemporaryExpired,
            HttpStatusCode.UnprocessableEntity,
            "The uploaded file expired, upload it again"),
        new(
            typeof(BlobNotFoundException),
            BlobErrorCodes.NotFound,
            HttpStatusCode.NotFound,
            "The file does not exist"),
        new(
            typeof(BlobContentTypeNotAllowedException),
            BlobEndpointErrorCodes.ContentTypeNotAllowed,
            HttpStatusCode.BadRequest,
            "The content type is missing, invalid or not allowed"),
        new(
            typeof(BlobClientKeyNotAllowedException),
            BlobEndpointErrorCodes.ClientKeyNotAllowed,
            HttpStatusCode.BadRequest,
            "Overwriting a key is not allowed"),
    ];

    /// <summary>
    /// El error de una excepción de blobs, o <c>null</c> si no es una.
    /// </summary>
    public static BlobHttpError? For(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return All.FirstOrDefault(e => e.ExceptionType == exception.GetType());
    }
}
