using CQ.ApiElements;

namespace CQ.AuthProvider.BusinessLogic.Tokens;

/// <summary>
/// The single handler of the "Bearer" scheme. Both token formats travel under
/// that same header, so registering <see cref="JwtTokenService"/> and
/// <see cref="GuidTokenService"/> as token services side by side would just
/// mean the second one never gets picked: the authentication filter resolves
/// the scheme with a FirstOrDefault. This dispatches on the format of the token
/// instead, which is what lets the two live together while the opaque tokens
/// issued before the switch drain out.
/// </summary>
/// <remarks>
/// Which format a new session gets is the caller's call, and the default is
/// still the opaque one: the move to JWT is opt in, so a client that does not
/// ask for anything keeps behaving exactly as it did before.
/// </remarks>
internal sealed class BearerTokenService(
    JwtTokenService jwtTokenService,
    GuidTokenService guidTokenService)
    : ITokenService
{
    public string AuthorizationTypeHandled => "Bearer";

    public Task<string> CreateAsync(object item)
    {
        var format = item is SessionTokenPayload payload
            ? payload.Format
            : TokenFormat.Opaque;

        var tokenService = format == TokenFormat.Jwt
            ? (ITokenService)jwtTokenService
            : guidTokenService;

        return tokenService.CreateAsync(item);
    }

    public async Task<bool> IsValidAsync(string value)
    {
        var isJwt = await jwtTokenService
            .IsValidAsync(value)
            .ConfigureAwait(false);

        if (isJwt)
        {
            return true;
        }

        return await guidTokenService
            .IsValidAsync(value)
            .ConfigureAwait(false);
    }

    public async Task<object?> GetOrDefaultAsync(string value)
    {
        // A JWT has three base64url segments, an opaque token is a bare guid,
        // so the formats never overlap.
        var isJwt = await jwtTokenService
            .IsValidAsync(value)
            .ConfigureAwait(false);

        var tokenService = isJwt
            ? (ITokenService)jwtTokenService
            : guidTokenService;

        return await tokenService
            .GetOrDefaultAsync(value)
            .ConfigureAwait(false);
    }
}
