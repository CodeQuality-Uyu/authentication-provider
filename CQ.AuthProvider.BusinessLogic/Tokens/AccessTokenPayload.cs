using System.Text.Json.Serialization;

namespace CQ.AuthProvider.BusinessLogic.Tokens;

/// <summary>
/// Typed view of the access token payload. Mirrors what
/// <see cref="JwtTokenService"/> writes, and is what an app validating the
/// token on its own would need to read.
/// </summary>
internal sealed record AccessTokenPayload
{
    [JsonPropertyName("sub")]
    public Guid AccountId { get; init; }

    [JsonPropertyName(JwtClaims.SessionId)]
    public Guid SessionId { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("given_name")]
    public string? FirstName { get; init; }

    [JsonPropertyName("family_name")]
    public string? LastName { get; init; }

    [JsonPropertyName("name")]
    public string? FullName { get; init; }

    [JsonPropertyName("locale")]
    public string? Locale { get; init; }

    [JsonPropertyName("zoneinfo")]
    public string? TimeZone { get; init; }

    [JsonPropertyName(JwtClaims.ProfilePictureKey)]
    public string? ProfilePictureKey { get; init; }

    [JsonPropertyName(JwtClaims.Tenant)]
    public TenantClaim? Tenant { get; init; }

    [JsonPropertyName(JwtClaims.AppLogged)]
    public AppClaim? AppLogged { get; init; }

    [JsonPropertyName(JwtClaims.Apps)]
    public List<AppClaim> Apps { get; init; } = [];

    [JsonPropertyName(JwtClaims.Roles)]
    public List<RoleClaim> Roles { get; init; } = [];
}
