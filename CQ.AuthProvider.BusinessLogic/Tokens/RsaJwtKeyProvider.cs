using System.Security.Cryptography;
using System.Text;
using CQ.AuthProvider.BusinessLogic.AppConfig;
using CQ.Utility;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CQ.AuthProvider.BusinessLogic.Tokens;

/// <summary>
/// Holds the RSA key pair used for RS256. Registered as a singleton: the key is
/// read (or generated) once and reused for every token.
/// </summary>
internal sealed class RsaJwtKeyProvider
    : IJwtKeyProvider,
    IDisposable
{
    private readonly RSA _rsa;

    public SigningCredentials SigningCredentials { get; }

    public IReadOnlyList<SecurityKey> ValidationKeys { get; }

    public IReadOnlyList<JwtPublicKey> PublicKeys { get; }

    public bool IsEphemeral { get; }

    public RsaJwtKeyProvider(IOptions<JwtSection> options)
    {
        var jwt = options.Value;

        (_rsa, IsEphemeral) = BuildRsa(jwt.PrivateKeyPem);

        var parameters = _rsa.ExportParameters(includePrivateParameters: false);
        var keyId = Guard.IsNullOrEmpty(jwt.KeyId)
            ? BuildKeyId(parameters)
            : jwt.KeyId!;

        var securityKey = new RsaSecurityKey(_rsa) { KeyId = keyId };

        SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
        ValidationKeys = [securityKey];
        PublicKeys =
        [
            new JwtPublicKey
            {
                KeyId = keyId,
                Modulus = Base64UrlEncoder.Encode(parameters.Modulus),
                Exponent = Base64UrlEncoder.Encode(parameters.Exponent)
            }
        ];
    }

    private static (RSA Rsa, bool IsEphemeral) BuildRsa(string? privateKeyPem)
    {
        if (Guard.IsNullOrEmpty(privateKeyPem))
        {
            return (RSA.Create(2048), true);
        }

        var rsa = RSA.Create();
        rsa.ImportFromPem(NormalizePem(privateKeyPem!));

        return (rsa, false);
    }

    /// <summary>
    /// Accepts the key either as a literal PEM or base64 encoded, since PEM
    /// newlines rarely survive a trip through an environment variable.
    /// </summary>
    private static string NormalizePem(string privateKey)
    {
        var trimmed = privateKey.Trim();

        if (trimmed.Contains("-----BEGIN", StringComparison.Ordinal))
        {
            return trimmed.Replace("\\n", "\n", StringComparison.Ordinal);
        }

        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(trimmed));
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                $"{JwtSection.SectionName}:{nameof(JwtSection.PrivateKeyPem)} is neither a PEM block nor valid base64",
                exception);
        }
    }

    /// <summary>
    /// Thumbprint of the public key, so the kid stays the same across restarts
    /// and instances as long as the key does.
    /// </summary>
    private static string BuildKeyId(RSAParameters parameters)
    {
        var material = new byte[parameters.Modulus!.Length + parameters.Exponent!.Length];
        parameters.Modulus.CopyTo(material, 0);
        parameters.Exponent.CopyTo(material, parameters.Modulus.Length);

        return Base64UrlEncoder.Encode(SHA256.HashData(material))[..16];
    }

    public void Dispose()
    {
        _rsa.Dispose();
    }
}
