namespace CQ.Blobs;

/// <summary>
/// Manejo de blobs de una API.
/// </summary>
/// <remarks>
/// Flujo de subida de un archivo que pertenece a un recurso:
/// <list type="number">
/// <item>El cliente reserva con <see cref="CreateUploadAsync"/> y sube con un PUT a
/// <see cref="BlobReadWrite.WriteUrl"/>. El objeto queda en la carpeta temporal.</item>
/// <item>El cliente manda la key al endpoint del recurso.</item>
/// <item>El recurso la mueve a su carpeta con <see cref="PromoteAsync"/> (alta) o con
/// <see cref="StageReplacementAsync"/> + <see cref="CommitAsync"/> (edición).</item>
/// </list>
/// Lo que nunca se promueve lo limpia la lifecycle rule de la carpeta temporal.
/// </remarks>
public interface IBlobService
{
    /// <summary>
    /// Reserva <c>{temporary}/{scope…}/{guid}.{ext}</c> y devuelve la URL para subirlo.
    /// </summary>
    /// <param name="contentType">El mismo que tiene que llevar el PUT.</param>
    /// <param name="scope">
    /// Opcional. Segmentos que identifican a quién pertenece la subida (tenant, app). Hace
    /// falta cuando varios clientes comparten el bucket; con un bucket por cliente, no se
    /// pasa. Se normalizan con <see cref="BlobKey.Slug"/>. Son los que después exige
    /// <see cref="PromoteAsync"/> en <c>requiredScope</c>.
    /// </param>
    Task<BlobReadWrite> CreateUploadAsync(string contentType, params string[] scope);

    /// <summary>
    /// Devuelve una URL de escritura para una key exacta, que puede pisar un objeto que ya
    /// existe.
    /// </summary>
    /// <remarks>
    /// ⚠️ La key tiene que armarla el servidor. Con una key que manda el cliente, cualquiera
    /// puede sobrescribir cualquier objeto del bucket.
    /// </remarks>
    Task<BlobReadWrite> CreateUploadForKeyAsync(string key, string contentType);

    /// <summary>
    /// URL de lectura: pública por CDN si hay <see cref="BlobOptions.CloudFrontDomain"/>,
    /// si no, prefirmada (vence a los <see cref="BlobOptions.PresignedUrlExpiration"/>).
    /// </summary>
    BlobRead GetByKey(string key);

    /// <summary>
    /// URL prefirmada de lectura, aunque haya CDN. Para lo que no tiene que ser público.
    /// </summary>
    BlobRead GetPresignedByKey(string key, PresignedReadOptions? options = null);

    bool IsTemporaryKey(string key);

    /// <summary>
    /// Si la key pertenece al alcance: está en <c>{scope…}/</c> (definitiva) o en
    /// <c>{temporary}/{scope…}/</c> (subida pendiente). Sin alcance, toda key pertenece.
    /// </summary>
    /// <remarks>
    /// Una key con segmentos vacíos o relativos nunca pertenece a un alcance.
    /// </remarks>
    bool IsInScope(string key, params string[] scope);

    /// <summary>
    /// Copia un temporal a <c>{destinationFolder}/{archivo}</c> y devuelve la key nueva.
    /// El temporal no se borra: lo limpia la lifecycle rule.
    /// </summary>
    /// <param name="temporaryKey">Key que devolvió <see cref="CreateUploadAsync"/>.</param>
    /// <param name="destinationFolder">
    /// Una o más carpetas (<c>"tenant/app"</c>). Cada segmento se normaliza.
    /// </param>
    /// <param name="requiredScope">
    /// Si se pasa, la key tiene que estar en <c>{temporary}/{requiredScope…}/</c>. Es lo
    /// que impide promover la subida de otro tenant.
    /// </param>
    /// <exception cref="BlobKeyNotInTemporaryException">La key no es temporal.</exception>
    /// <exception cref="BlobKeyOutOfScopeException">La key es de otro alcance.</exception>
    /// <exception cref="BlobTemporaryExpiredException">El temporal ya no existe.</exception>
    Task<string> PromoteAsync(string temporaryKey, string destinationFolder, params string[] requiredScope);

    /// <summary>
    /// Primera fase de un reemplazo. Con <paramref name="incomingKey"/> vacía o igual a la
    /// actual no toca nada (el recurso conserva lo que tenía). Con una key nueva, la
    /// promueve.
    /// </summary>
    /// <remarks>
    /// Persistir <see cref="BlobReplacement.Key"/> y después llamar a
    /// <see cref="CommitAsync"/>, o a <see cref="RollbackAsync"/> si el guardado falló.
    /// Para quitar el blob, usar <see cref="BlobReplacement.Removal"/>.
    /// </remarks>
    Task<BlobReplacement> StageReplacementAsync(
        string? incomingKey,
        string? currentKey,
        string destinationFolder,
        params string[] requiredScope);

    /// <summary>
    /// Borra el blob reemplazado. Se llama después de persistir. Si el borrado falla, lo
    /// registra y sigue: el recurso ya quedó bien, y un objeto huérfano no justifica
    /// responder error.
    /// </summary>
    Task CommitAsync(BlobReplacement replacement);

    /// <summary>
    /// Borra el blob recién promovido, para cuando el guardado falló. Tampoco propaga
    /// errores, para no tapar el del guardado.
    /// </summary>
    Task RollbackAsync(BlobReplacement replacement);

    /// <summary>
    /// Sube un contenido generado en el servidor. No cierra el stream.
    /// </summary>
    Task UploadAsync(string key, Stream content, string contentType);

    /// <summary>
    /// Descarga el objeto entero a memoria.
    /// </summary>
    /// <exception cref="BlobNotFoundException">El objeto no existe.</exception>
    Task<MemoryStream> OpenReadAsync(string key);

    Task DeleteAsync(string key);
}
