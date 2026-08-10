using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CQ.AuthProvider.BusinessLogic.Sessions;

/// <summary>
/// The refresh token is opaque and long lived, so it is generated with a
/// cryptographic RNG and only its hash reaches the database: a dump of the
/// Sessions table is not enough to impersonate anyone.
/// </summary>
internal static class RefreshTokenFactory
{
    private const int SizeInBytes = 32;

    public static string New()
    {
        return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(SizeInBytes));
    }

    public static string Hash(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));

        return Convert.ToHexString(bytes);
    }
}
