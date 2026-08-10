using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Tokens;

namespace CQ.AuthProvider.BusinessLogic.Sessions;

public sealed record class Session()
{
    /// <summary>
    /// Also travels in the access token as the <c>sid</c> claim, which is how
    /// logout knows which session to revoke.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Which of the two shapes this session has. Decides what gets persisted and
    /// what the client can do with the token.
    /// </summary>
    public TokenFormat TokenFormat { get; init; }

    /// <summary>
    /// The access token. A self contained JWT when
    /// <see cref="TokenFormat"/> is <see cref="Tokens.TokenFormat.Jwt"/>, in
    /// which case it is not persisted; an opaque guid otherwise, and that one
    /// does live in the database.
    /// </summary>
    public string Token { get; init; } = null!;

    /// <summary>
    /// Null on opaque sessions, which do not expire.
    /// </summary>
    public DateTime? TokenExpiresAt { get; init; }

    /// <summary>
    /// Long lived token handed to the client, only on JWT sessions. Only its
    /// hash is stored, so this value is populated when the session is issued and
    /// empty when the session is read back from the database.
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Null on opaque sessions: those cannot be refreshed, only used until they
    /// are deleted.
    /// </summary>
    public string? RefreshTokenHash { get; init; }

    public DateTime? RefreshTokenExpiresAt { get; init; }

    public Account Account { get; init; } = null!;

    public App App { get; init; } = null!;

    /// <summary>
    /// App-specific data fetched from the app's own API at login time, when the
    /// app registered an <see cref="Apps.AccountDataSource"/>. Opaque to the auth
    /// provider — forwarded as-is in the login response. Null when the app has no
    /// source configured or the fetch failed.
    /// </summary>
    public object? AppData { get; init; }
}
