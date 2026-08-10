using CQ.ApiElements;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Sessions;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.Tokens;

/// <summary>
/// Opaque token backed by a row in Sessions. Superseded by
/// <see cref="JwtTokenService"/> for new logins, but kept alive so the tokens
/// handed out before the switch keep working until they are dropped: it is
/// reached through <see cref="BearerTokenService"/>, never registered on its own.
/// </summary>
public sealed class GuidTokenService(
    ISessionRepository sessionRepository)
    : ITokenService
{
    public string AuthorizationTypeHandled => "Bearer";

    public Task<string> CreateAsync(object item)
    {
        return Task.FromResult(Db.NewId());
    }

    public Task<bool> IsValidAsync(string value)
    {
        var isGuid = Db.IsIdValid(value);

        return Task.FromResult(isGuid);
    }

    public async Task<object?> GetOrDefaultAsync(string value)
    {
        var session = await sessionRepository
            .GetOrDefaultByTokenAsync(value)
            .ConfigureAwait(false);

        if (Guard.IsNull(session))
        {
            return null;
        }

        var account = new AccountLogged(
            session!.Account,
            value,
            session.App)
        {
            SessionId = session.Id
        };

        return account;
    }
}
