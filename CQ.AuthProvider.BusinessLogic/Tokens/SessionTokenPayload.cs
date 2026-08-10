using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;

namespace CQ.AuthProvider.BusinessLogic.Tokens;

/// <summary>
/// Everything an access token needs to carry, plus which format to mint it in.
/// <see cref="ITokenService.CreateAsync"/> takes an <see cref="object"/>, so
/// this is the contract <see cref="BearerTokenService"/> expects to receive.
/// </summary>
public sealed record SessionTokenPayload(
    Account Account,
    App App,
    Guid SessionId,
    TokenFormat Format = TokenFormat.Opaque);
