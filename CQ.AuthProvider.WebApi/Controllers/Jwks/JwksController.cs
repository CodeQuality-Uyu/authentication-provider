using CQ.AuthProvider.BusinessLogic.Tokens;
using Microsoft.AspNetCore.Mvc;

namespace CQ.AuthProvider.WebApi.Controllers.Jwks;

/// <summary>
/// Publishes the public half of the signing keys so any app can validate an
/// access token on its own, without calling back into this provider.
/// </summary>
[ApiController]
[Route(".well-known")]
public sealed class JwksController(IJwtKeyProvider keyProvider)
    : ControllerBase
{
    [HttpGet("jwks.json")]
    public JwksResponse Get()
    {
        return new JwksResponse(keyProvider.PublicKeys);
    }
}
