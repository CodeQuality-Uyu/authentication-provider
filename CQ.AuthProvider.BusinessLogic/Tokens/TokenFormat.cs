using System.Text.Json.Serialization;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CQ.AuthProvider.BusinessLogic.Tokens;

/// <summary>
/// Which token service minted an access token. Both formats travel under the
/// same "Bearer" scheme, so this is the only thing that tells the client how to
/// treat the token it just got.
/// </summary>
/// <remarks>
/// Serialized by name rather than by ordinal, so it reads as "Jwt" on the wire.
/// The converter is declared here instead of globally on purpose: turning it on
/// for every enum would also rewrite <c>ErrorResponse.StatusCode</c> from 401 to
/// "Unauthorized" and break every error consumer.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TokenFormat
{
    /// <summary>
    /// Opaque guid backed by a row in Sessions, minted by
    /// <see cref="GuidTokenService"/>. The default: a client that says nothing
    /// keeps getting exactly what it got before JWT existed. Cannot be read by
    /// the client, does not expire, and has no refresh token.
    /// </summary>
    Opaque,

    /// <summary>
    /// Self contained RS256 JWT minted by <see cref="JwtTokenService"/>. Opt in:
    /// the client has to ask for it. Its claims can be read and its signature
    /// validated against /.well-known/jwks.json, it expires, and it comes with a
    /// refresh token.
    /// </summary>
    Jwt
}

public static class TokenFormats
{
    private static readonly JsonWebTokenHandler _handler = new();

    /// <summary>
    /// Read off the token itself, the same rule <see cref="BearerTokenService"/>
    /// dispatches on: a JWT has three base64url segments, every other token
    /// handed out by this provider is a bare guid, so the formats never overlap.
    /// </summary>
    public static TokenFormat Of(string token)
    {
        return _handler.CanReadToken(token)
            ? TokenFormat.Jwt
            : TokenFormat.Opaque;
    }
}
