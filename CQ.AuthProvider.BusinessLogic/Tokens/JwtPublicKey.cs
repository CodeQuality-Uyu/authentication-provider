using System.Text.Json.Serialization;

namespace CQ.AuthProvider.BusinessLogic.Tokens;

/// <summary>
/// Public half of a signing key, shaped as a JWK (RFC 7517) so apps can validate
/// the access tokens on their own instead of calling back into this provider.
/// </summary>
public sealed record JwtPublicKey
{
    [JsonPropertyName("kty")]
    public string KeyType { get; init; } = "RSA";

    [JsonPropertyName("use")]
    public string Use { get; init; } = "sig";

    [JsonPropertyName("alg")]
    public string Algorithm { get; init; } = "RS256";

    [JsonPropertyName("kid")]
    public string KeyId { get; init; } = null!;

    /// <summary>
    /// Modulus, base64url encoded.
    /// </summary>
    [JsonPropertyName("n")]
    public string Modulus { get; init; } = null!;

    /// <summary>
    /// Exponent, base64url encoded.
    /// </summary>
    [JsonPropertyName("e")]
    public string Exponent { get; init; } = null!;
}
