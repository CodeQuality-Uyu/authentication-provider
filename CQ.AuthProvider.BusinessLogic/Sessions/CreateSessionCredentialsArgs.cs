using CQ.AuthProvider.BusinessLogic.Tokens;

namespace CQ.AuthProvider.BusinessLogic.Sessions;

/// <param name="TokenFormat">
/// Which kind of access token to issue. Omitting it yields
/// <see cref="Tokens.TokenFormat.Opaque"/>, the format handed out before JWT
/// existed, so a client that does not know about this field is unaffected.
/// Send "Jwt" to opt into the self contained token and its refresh flow.
/// </param>
public sealed record CreateSessionCredentialsArgs(
    string Email,
    string Password,
    Guid AppId,
    TokenFormat TokenFormat = TokenFormat.Opaque);
