namespace CQ.Blobs;

/// <summary>
/// Una key y la URL para leerla.
/// </summary>
public sealed record BlobRead
{
    public string Key { get; init; } = null!;

    public string Url { get; init; } = null!;
}

/// <summary>
/// Una key reservada, la URL para leerla y la URL prefirmada para subirla con un PUT.
/// </summary>
/// <remarks>
/// El PUT a <see cref="WriteUrl"/> tiene que llevar el mismo <c>Content-Type</c> con el
/// que se reservó y <c>x-amz-server-side-encryption: AES256</c>. Si no, S3 responde 403.
/// </remarks>
public sealed record BlobReadWrite(
    string Key,
    string ReadUrl,
    string WriteUrl);

/// <summary>
/// Cómo servir un objeto en una URL prefirmada de lectura.
/// </summary>
public sealed record PresignedReadOptions
{
    /// <summary>
    /// Con valor, el navegador lo muestra en su visor (<c>inline</c>) y propone este nombre
    /// al guardarlo.
    /// </summary>
    public string? InlineFileName { get; init; }

    /// <summary>
    /// Pisa el <c>Content-Type</c> guardado. Hace falta cuando el objeto se subió con uno
    /// genérico y el visor necesita el real (por ejemplo, <c>application/pdf</c>).
    /// </summary>
    public string? ContentType { get; init; }
}

/// <summary>
/// Resultado de <see cref="IBlobService.StageReplacementAsync"/>: la key que hay que
/// persistir y la que queda reemplazada.
/// </summary>
/// <remarks>
/// El reemplazo se hace en dos fases para no dejar al recurso apuntando a un objeto que
/// no existe. Primero se promueve lo nuevo y se persiste <see cref="Key"/>. Si el guardado
/// sale bien, <see cref="IBlobService.CommitAsync"/> borra lo anterior. Si falla,
/// <see cref="IBlobService.RollbackAsync"/> borra lo recién promovido.
/// </remarks>
public sealed record BlobReplacement(
    string? Key,
    string? PreviousKey)
{
    public bool HasChanged => Key != PreviousKey;

    /// <summary>
    /// Sin cambios: el recurso conserva la key que tenía.
    /// </summary>
    public static BlobReplacement Unchanged(string? currentKey) => new(currentKey, currentKey);

    /// <summary>
    /// El recurso se queda sin blob. Hay que persistir <c>null</c>, y el commit borra el
    /// anterior.
    /// </summary>
    public static BlobReplacement Removal(string? currentKey) => new(null, currentKey);
}
