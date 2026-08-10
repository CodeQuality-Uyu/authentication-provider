using CQ.AuthProvider.BusinessLogic.Blobs;
using CQ.AuthProvider.BusinessLogic.Tokens;
using CQ.AuthProvider.WebApi.Controllers.Tenants;

namespace CQ.AuthProvider.WebApi.Controllers.Sessions;

public readonly struct SessionCreatedResponse
{
    public Guid Id { get; init; }

    public BlobReadResponse? ProfilePicture { get; init; }

    public string Email { get; init; }

    public string FirstName { get; init; }

    public string LastName { get; init; }

    public string FullName { get; init; }

    /// <summary>
    /// Access token, ready to be sent back as the Authorization header.
    /// </summary>
    public string Token { get; init; }

    /// <summary>
    /// Which token service minted <see cref="Token"/>, so the client knows how
    /// to treat it. "Opaque": the historical format, does not expire, cannot be
    /// read by the client, and only POST /sessions/check can tell whether it is
    /// still valid. "Jwt": self contained, expires, its claims can be read and
    /// its signature validated against /.well-known/jwks.json, and it comes
    /// with a <see cref="RefreshToken"/>.
    /// </summary>
    public TokenFormat TokenFormat { get; init; }

    /// <summary>
    /// Seconds the access token remains valid. Null when there is nothing to
    /// count down: an opaque token, which does not expire, or GET /me, which
    /// reports the account behind an already issued token.
    /// </summary>
    public int? ExpiresIn { get; init; }

    /// <summary>
    /// Opaque token used to obtain a new access token at
    /// POST /sessions/refresh. Store it where the access token is not reachable
    /// by scripts. Null on opaque sessions, which have nothing to refresh, and
    /// on GET /me.
    /// </summary>
    public string? RefreshToken { get; init; }

    public List<string> Roles { get; init; }

    public List<string> Permissions { get; init; }

    public SessionAppLoggedResponse AppLogged { get; init; }

    public TenantOfAccountBasicInfoResponse Tenant { get; init; }

    /// <summary>
    /// App-specific data fetched from the logged app's own API at login time.
    /// Null when the app registered no data source or the fetch failed. Its
    /// shape is defined entirely by the app, not by this provider.
    /// </summary>
    public object? AppData { get; init; }
}

public sealed record SessionAppLoggedResponse(
    Guid Id,
    string Name);