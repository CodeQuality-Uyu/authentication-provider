namespace CQ.Blobs.AspNetCore;

/// <summary>
/// Sección <c>Blob:Endpoints</c>.
/// </summary>
public sealed record BlobEndpointOptions
{
    public const string Section = "Blob:Endpoints";

    /// <summary>
    /// Content types que se pueden reservar. Acepta comodines por tipo (<c>image/*</c>).
    /// Vacía, se acepta cualquiera con formato válido.
    /// </summary>
    public IList<string> AllowedContentTypes { get; init; } = [];

    /// <summary>
    /// Si <c>POST /blobs</c> acepta una key del cliente para sobrescribir un archivo en el
    /// lugar. Apagado por defecto: con una key del cliente se puede pisar cualquier objeto
    /// del alcance.
    /// </summary>
    /// <remarks>
    /// Existe por los fronts que todavía reemplazan reusando la key (el modo
    /// <c>keyOnReplace: 'reuse'</c> del front de licenciatario de eColors). El reemplazo
    /// seguro es reservar una key nueva y que el recurso la promueva con
    /// <see cref="IBlobService.StageReplacementAsync"/>.
    /// </remarks>
    public bool AllowClientKey { get; init; }
}
