namespace CQ.AuthProvider.BusinessLogic.Sessions;

public interface ISessionRepository
{
    Task CreateAsync(Session session);

    /// <summary>
    /// Loads a session by its opaque token. Only sessions created before the
    /// switch to JWT have one. Null when the token is unknown.
    /// </summary>
    Task<Session?> GetOrDefaultByTokenAsync(string token);

    /// <summary>
    /// Loads the session behind a refresh token, along with the account and app
    /// needed to mint a new access token. Null when the hash is unknown.
    /// </summary>
    Task<Session?> GetOrDefaultByRefreshTokenHashAsync(string refreshTokenHash);

    /// <summary>
    /// Rotates the refresh token of a session that is being refreshed.
    /// </summary>
    Task UpdateRefreshTokenAsync(
        Guid sessionId,
        string refreshTokenHash,
        DateTime refreshTokenExpiresAt);

    Task DeleteByIdAsync(Guid sessionId);
}
