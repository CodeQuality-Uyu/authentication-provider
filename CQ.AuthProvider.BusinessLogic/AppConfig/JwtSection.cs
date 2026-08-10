namespace CQ.AuthProvider.BusinessLogic.AppConfig;

public sealed record JwtSection
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "cq-auth-provider";

    /// <summary>
    /// RSA private key used to sign the access tokens, in PKCS#8 PEM format. It
    /// can also be provided base64 encoded, which is friendlier for environment
    /// variables. When empty an ephemeral key is generated at startup, which is
    /// only acceptable outside of production: tokens stop validating on restart
    /// and every instance signs with a different key.
    /// </summary>
    public string? PrivateKeyPem { get; init; }

    /// <summary>
    /// Identifier published in the token header and in the JWKS document. When
    /// empty it is derived from the public key, so it stays stable as long as
    /// the key does.
    /// </summary>
    public string? KeyId { get; init; }

    public int AccessTokenExpirationInMinutes { get; init; } = 15;

    public int RefreshTokenExpirationInDays { get; init; } = 30;

    public int ClockSkewInSeconds { get; init; } = 30;
}
