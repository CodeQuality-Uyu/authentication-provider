using CQ.ApiElements;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.AppConfig;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Identities;
using CQ.AuthProvider.BusinessLogic.Sessions.Exceptions;
using CQ.AuthProvider.BusinessLogic.Tokens;
using CQ.UnitOfWork.Abstractions;
using CQ.Utility;
using Microsoft.Extensions.Options;

namespace CQ.AuthProvider.BusinessLogic.Sessions;

public sealed class SessionService(
    ISessionRepository sessionRepository,
    IIdentityRepository identityRepository,
    IAccountRepository accountRepository,
    ITokenService tokenService,
    IAccountDataEnricher accountDataEnricher,
    IOptions<JwtSection> jwtOptions,
    IUnitOfWork _unitOfWork)
    : ISessionInternalService
{
    private readonly JwtSection _jwt = jwtOptions.Value;

    public async Task<Session> CreateAsync(CreateSessionCredentialsArgs args)
    {
        var identity = await identityRepository
            .GetByCredentialsAsync(args.Email, args.Password)
            .ConfigureAwait(false);

        var account = await accountRepository
            .GetByIdAsync(identity.Id, args.AppId)
            .ConfigureAwait(true);

        var app = account
            .Apps
            .FirstOrDefault(a => a.Id == args.AppId);

        if (Guard.IsNull(app))
        {
            throw new InvalidOperationException($"Account ({account.Email}) doesn't exist in app ({args.AppId})");
        }

        var session = await CreateAsync(
            account,
            app!,
            args.TokenFormat)
            .ConfigureAwait(false);

        await _unitOfWork
            .CommitChangesAsync()
            .ConfigureAwait(false);

        // Enriched after commit so the app's data endpoint sees a persisted
        // session. The access token no longer needs it to validate, but the
        // ordering costs nothing and keeps the callback safe either way.
        var appData = await accountDataEnricher
            .GetAsync(app, session.Token)
            .ConfigureAwait(false);

        return session with { AppData = appData };
    }

    public async Task<Session> CreateAsync(
        Account account,
        App app,
        TokenFormat tokenFormat = TokenFormat.Opaque)
    {
        var sessionId = Guid.NewGuid();

        var token = await tokenService
            .CreateAsync(new SessionTokenPayload(
            account,
            app,
            sessionId,
            tokenFormat))
            .ConfigureAwait(false);

        var session = tokenFormat == TokenFormat.Jwt
            ? BuildJwtSession(sessionId, account, app, token)
            : BuildOpaqueSession(sessionId, account, app, token);

        await sessionRepository
            .CreateAsync(session)
            .ConfigureAwait(false);

        return session;
    }

    /// <summary>
    /// Expires, and carries a refresh token to outlive that expiration. The
    /// access token itself never reaches the database.
    /// </summary>
    private Session BuildJwtSession(
        Guid sessionId,
        Account account,
        App app,
        string token)
    {
        var now = DateTime.UtcNow;
        var refreshToken = RefreshTokenFactory.New();

        return new Session
        {
            Id = sessionId,
            TokenFormat = TokenFormat.Jwt,
            Account = account,
            App = app,
            Token = token,
            TokenExpiresAt = now.AddMinutes(_jwt.AccessTokenExpirationInMinutes),
            RefreshToken = refreshToken,
            RefreshTokenHash = RefreshTokenFactory.Hash(refreshToken),
            RefreshTokenExpiresAt = now.AddDays(_jwt.RefreshTokenExpirationInDays)
        };
    }

    /// <summary>
    /// What every session looked like before JWT: the token is stored as is,
    /// never expires, and there is nothing to refresh. Kept as the default so
    /// clients that do not ask for a format see no change at all.
    /// </summary>
    private static Session BuildOpaqueSession(
        Guid sessionId,
        Account account,
        App app,
        string token)
    {
        return new Session
        {
            Id = sessionId,
            TokenFormat = TokenFormat.Opaque,
            Account = account,
            App = app,
            Token = token
        };
    }

    public async Task<Session> RefreshAsync(RefreshSessionArgs args)
    {
        var hash = RefreshTokenFactory.Hash(args.RefreshToken);

        var stored = await sessionRepository
            .GetOrDefaultByRefreshTokenHashAsync(hash)
            .ConfigureAwait(false);

        if (Guard.IsNull(stored))
        {
            throw new InvalidRefreshTokenException();
        }

        if (stored!.RefreshTokenExpiresAt == null ||
            stored.RefreshTokenExpiresAt <= DateTime.UtcNow)
        {
            await sessionRepository
                .DeleteByIdAsync(stored.Id)
                .ConfigureAwait(false);

            throw new InvalidRefreshTokenException();
        }

        // Minted from what the database holds now, so role and permission
        // changes take effect on the next refresh instead of on the next login.
        // Always a JWT: only a JWT session has a refresh token to get here with.
        var token = await tokenService
            .CreateAsync(new SessionTokenPayload(
            stored.Account,
            stored.App,
            stored.Id,
            TokenFormat.Jwt))
            .ConfigureAwait(false);

        var now = DateTime.UtcNow;
        var refreshToken = RefreshTokenFactory.New();
        var refreshTokenHash = RefreshTokenFactory.Hash(refreshToken);
        var refreshTokenExpiresAt = now.AddDays(_jwt.RefreshTokenExpirationInDays);

        await sessionRepository
            .UpdateRefreshTokenAsync(
            stored.Id,
            refreshTokenHash,
            refreshTokenExpiresAt)
            .ConfigureAwait(false);

        return stored with
        {
            TokenFormat = TokenFormat.Jwt,
            Token = token,
            TokenExpiresAt = now.AddMinutes(_jwt.AccessTokenExpirationInMinutes),
            RefreshToken = refreshToken,
            RefreshTokenHash = refreshTokenHash,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };
    }

    /// <summary>
    /// Drops the refresh token, so the session cannot be extended. The access
    /// token already handed out stays valid until it expires, which is the
    /// trade-off of validating it without touching the database.
    /// </summary>
    public async Task DeleteAsync(AccountLogged accountLogged)
    {
        await sessionRepository
            .DeleteByIdAsync(accountLogged.SessionId)
            .ConfigureAwait(false);
    }
}
