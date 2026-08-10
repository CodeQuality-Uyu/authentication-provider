using Microsoft.IdentityModel.Tokens;

namespace CQ.AuthProvider.BusinessLogic.Tokens;

public interface IJwtKeyProvider
{
    /// <summary>
    /// Key used to sign newly issued access tokens.
    /// </summary>
    SigningCredentials SigningCredentials { get; }

    /// <summary>
    /// Keys accepted when validating an incoming access token. Holds more than
    /// one entry only while a key rotation is in flight.
    /// </summary>
    IReadOnlyList<SecurityKey> ValidationKeys { get; }

    /// <summary>
    /// Public half of <see cref="ValidationKeys"/>, published at
    /// /.well-known/jwks.json.
    /// </summary>
    IReadOnlyList<JwtPublicKey> PublicKeys { get; }

    /// <summary>
    /// True when the key was generated at startup instead of read from
    /// configuration. Tokens signed with it die on restart.
    /// </summary>
    bool IsEphemeral { get; }
}
