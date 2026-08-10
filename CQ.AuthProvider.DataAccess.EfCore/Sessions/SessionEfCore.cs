using CQ.AuthProvider.BusinessLogic.Sessions;
using CQ.AuthProvider.BusinessLogic.Tokens;
using CQ.AuthProvider.DataAccess.EfCore.Accounts;
using CQ.AuthProvider.DataAccess.EfCore.Apps;

namespace CQ.AuthProvider.DataAccess.EfCore.Sessions;

/// <summary>
/// Holds one of two shapes, never both. Sessions created before the switch to
/// JWT store their opaque access token in <see cref="Token"/>; the ones created
/// from then on store only the hash of their refresh token, since the access
/// token is a self contained JWT that never reaches the database.
/// </summary>
public sealed record class SessionEfCore()
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Opaque access token. Null on sessions backed by a JWT.
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    /// SHA-256 of the refresh token, hex encoded. The token itself is only ever
    /// held by the client. Null on the sessions that predate JWT.
    /// </summary>
    public string? RefreshTokenHash { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public Guid AppId { get; init; }

    public AppEfCore App { get; init; } = null!;

    public Guid AccountId { get; init; }

    public AccountEfCore Account { get; init; } = null!;

    // For new Session
    public SessionEfCore(
        Guid appId,
        string? token,
        string? refreshTokenHash,
        DateTime? refreshTokenExpiresAt,
        Guid accountId)
        : this()
    {
        AppId = appId;
        Token = token;
        RefreshTokenHash = refreshTokenHash;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
        AccountId = accountId;
    }

    internal SessionEfCore(Session session)
        : this(session.App.Id,
              // A JWT is self contained, so there is nothing worth storing and
              // storing it would only widen the blast radius of a table dump.
              // An opaque token is the opposite: the row is the only thing that
              // makes it mean anything.
              session.TokenFormat == TokenFormat.Jwt ? null : session.Token,
              session.RefreshTokenHash,
              session.RefreshTokenExpiresAt,
              session.Account.Id)
    {
        Id = session.Id;
    }
}
