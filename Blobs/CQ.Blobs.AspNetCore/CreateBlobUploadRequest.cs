namespace CQ.Blobs.AspNetCore;

/// <summary>
/// Cuerpo de <c>POST /blobs</c>. Una API puede heredarlo para sumar lo que necesita para
/// resolver el alcance (el Auth Provider suma <c>appId</c>).
/// </summary>
public record CreateBlobUploadRequest
{
    /// <summary>
    /// El mismo que tiene que llevar el PUT a la URL de escritura.
    /// </summary>
    /// <remarks>
    /// Anulable a propósito: si fuera <c>string</c>, MVC lo trataría como requerido y
    /// respondería su propio 400 antes de llegar al controller. Así la falta del campo sale
    /// como <see cref="BlobContentTypeNotAllowedException"/>, con el formato de error de la API.
    /// </remarks>
    public string? ContentType { get; init; }

    /// <summary>
    /// Key a sobrescribir, para reemplazar un archivo en el lugar. Sólo se acepta con
    /// <see cref="BlobEndpointOptions.AllowClientKey"/> y dentro del alcance de lectura.
    /// Vacía, se reserva una key temporal nueva.
    /// </summary>
    public string? Key { get; init; }
}
