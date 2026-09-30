using CQ.ApiElements.Filters.Authentications;
using CQ.AuthProvider.WebApi.Extensions;
using CQ.Blobs.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace CQ.AuthProvider.WebApi.Controllers.Blobs;

/// <summary>
/// <c>POST /blobs</c> y <c>GET /blobs/{**key}</c>, de CQ.Blobs.AspNetCore.
/// </summary>
/// <remarks>
/// El bucket es compartido por todos los tenants, así que todo va con alcance: las subidas
/// caen en <c>temporary/{tenant}/{app}/</c>, y sólo se leen o sobrescriben keys del propio
/// tenant (de cualquiera de sus apps).
/// </remarks>
[Route("blobs")]
[BearerAuthentication]
public sealed class BlobController
    : BlobControllerBase<CreateBlobRequest>
{
    /// <remarks>
    /// La app es la de <c>appId</c>, que tiene que ser de la cuenta, o la logueada.
    /// </remarks>
    protected override Task<string[]> ResolveUploadScopeAsync(CreateBlobRequest request)
    {
        var accountLogged = this.GetAccountLogged();

        var app = request.AppId is null
            ? accountLogged.AppLogged
            : accountLogged.Apps.FirstOrDefault(a => a.Id == request.AppId)
              ?? throw new InvalidOperationException("The app does not belong to the logged account.");

        return Task.FromResult(new[] { accountLogged.Tenant.Name, app.Name });
    }

    protected override Task<string[]> ResolveReadScopeAsync()
        => Task.FromResult(new[] { this.GetAccountLogged().Tenant.Name });
}
