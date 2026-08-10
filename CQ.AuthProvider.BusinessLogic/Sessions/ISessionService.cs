using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Tokens;

namespace CQ.AuthProvider.BusinessLogic.Sessions;

public interface ISessionService
{
    Task<Session> CreateAsync(CreateSessionCredentialsArgs args);

    /// <summary>
    /// Exchanges a refresh token for a fresh access token, rotating the refresh
    /// token in the process.
    /// </summary>
    Task<Session> RefreshAsync(RefreshSessionArgs args);

    Task DeleteAsync(AccountLogged accountLogged);
}

public interface ISessionInternalService
    : ISessionService
{
    /// <param name="tokenFormat">
    /// Defaults to <see cref="TokenFormat.Opaque"/> so callers that have not
    /// been migrated keep issuing exactly the token they used to.
    /// </param>
    Task<Session> CreateAsync(
        Account account,
        App app,
        TokenFormat tokenFormat = TokenFormat.Opaque);
}
