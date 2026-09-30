using CQ.Blobs.AspNetCore;

namespace CQ.AuthProvider.WebApi.Controllers.Blobs;

public sealed record CreateBlobRequest
    : CreateBlobUploadRequest
{
    /// <summary>
    /// App bajo la que se reserva la subida. Tiene que ser una app de la cuenta. Vacía, la
    /// app logueada.
    /// </summary>
    public Guid? AppId { get; init; }
}
