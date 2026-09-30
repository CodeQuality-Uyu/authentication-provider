using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CQ.Blobs.AspNetCore;

/// <summary>
/// Endpoints de blobs: <c>POST</c> reserva una subida y <c>GET {**key}</c> devuelve la URL
/// de lectura.
/// </summary>
/// <remarks>
/// No tiene ruta ni autenticación: cada API los pone en su controller, porque cada una se
/// autentica con sus propios filtros.
/// <code>
/// [Route("blobs")]
/// [BearerAuthentication]
/// public sealed class BlobController : BlobControllerBase&lt;CreateBlobUploadRequest&gt;;
/// </code>
/// Con varios clientes en el mismo bucket, sobrescribir <see cref="ResolveUploadScopeAsync"/>
/// y <see cref="ResolveReadScopeAsync"/>.
/// </remarks>
[ApiController]
public abstract class BlobControllerBase<TRequest> : ControllerBase
    where TRequest : CreateBlobUploadRequest
{
    /// <summary>
    /// Reserva una subida. Sin <c>key</c>, en <c>{temporary}/{alcance}/{guid}.{ext}</c>.
    /// Con <c>key</c>, para sobrescribirla (ver <see cref="BlobEndpointOptions.AllowClientKey"/>).
    /// </summary>
    [HttpPost]
    public async Task<BlobReadWrite> CreateAsync(
        [FromBody] TRequest request,
        [FromServices] IBlobService blobService,
        [FromServices] IOptions<BlobEndpointOptions> options)
    {
        var contentType = AssertContentType(request.ContentType, options.Value);

        if (string.IsNullOrWhiteSpace(request.Key))
        {
            var scope = await ResolveUploadScopeAsync(request).ConfigureAwait(false);

            return await blobService
                .CreateUploadAsync(contentType, scope)
                .ConfigureAwait(false);
        }

        await AssertClientKeyAsync(request.Key, blobService, options.Value).ConfigureAwait(false);

        return await blobService
            .CreateUploadForKeyAsync(request.Key, contentType)
            .ConfigureAwait(false);
    }

    /// <remarks>
    /// Comodín de ruta y no parámetro común: las keys tienen barras
    /// (<c>temporary/{tenant}/{app}/{guid}.png</c>), que un parámetro común no matchea.
    /// </remarks>
    [HttpGet("{**key}")]
    public async Task<BlobRead> GetByKeyAsync(
        string key,
        [FromServices] IBlobService blobService)
    {
        var scope = await ResolveReadScopeAsync().ConfigureAwait(false);

        if (!blobService.IsInScope(key, scope))
        {
            throw new BlobKeyOutOfScopeException(key);
        }

        return blobService.GetByKey(key);
    }

    /// <summary>
    /// Segmentos bajo los que se reserva una subida (tenant, app). Por defecto ninguno: la
    /// API tiene un bucket por cliente.
    /// </summary>
    protected virtual Task<string[]> ResolveUploadScopeAsync(TRequest request)
        => Task.FromResult(Array.Empty<string>());

    /// <summary>
    /// Alcance de las keys que quien llama puede leer y sobrescribir (su tenant). Por
    /// defecto ninguno: puede todas.
    /// </summary>
    /// <remarks>
    /// Suele ser más amplio que el de subida (el tenant, sin la app), para que una cuenta
    /// pueda trabajar con los archivos de todas las apps de su tenant.
    /// </remarks>
    protected virtual Task<string[]> ResolveReadScopeAsync()
        => Task.FromResult(Array.Empty<string>());

    /// <returns>El content type tal cual vino: el PUT tiene que mandar exactamente ése.</returns>
    private static string AssertContentType(string? contentType, BlobEndpointOptions options)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new BlobContentTypeNotAllowedException(contentType);
        }

        try
        {
            // Valida el formato tipo/subtipo.
            BlobKey.Extension(contentType);
        }
        catch (ArgumentException)
        {
            throw new BlobContentTypeNotAllowedException(contentType);
        }

        if (options.AllowedContentTypes.Count == 0)
        {
            return contentType;
        }

        var mediaType = contentType.Split(';')[0].Trim();
        var isAllowed = options.AllowedContentTypes.Any(allowed => allowed.EndsWith("/*", StringComparison.Ordinal)
            ? mediaType.StartsWith(allowed[..^1], StringComparison.OrdinalIgnoreCase)
            : string.Equals(allowed, mediaType, StringComparison.OrdinalIgnoreCase));

        if (!isAllowed)
        {
            throw new BlobContentTypeNotAllowedException(contentType);
        }

        return contentType;
    }

    /// <remarks>
    /// Una key temporal del alcance también se acepta: un formulario que cambia el archivo
    /// antes de guardar vuelve a pedir la key que reservó recién.
    /// </remarks>
    private async Task AssertClientKeyAsync(
        string key,
        IBlobService blobService,
        BlobEndpointOptions options)
    {
        if (!options.AllowClientKey || !BlobKey.IsValid(key))
        {
            throw new BlobClientKeyNotAllowedException(key);
        }

        var scope = await ResolveReadScopeAsync().ConfigureAwait(false);

        if (!blobService.IsInScope(key, scope))
        {
            throw new BlobKeyOutOfScopeException(key);
        }
    }
}
