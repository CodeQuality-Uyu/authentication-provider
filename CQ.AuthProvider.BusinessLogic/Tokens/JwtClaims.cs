using System.Text.Json.Serialization;

namespace CQ.AuthProvider.BusinessLogic.Tokens;

/// <summary>
/// Names of the non standard claims carried by the access token. The registered
/// ones (sub, jti, iat, exp, iss, aud, email, ...) come from
/// <c>JwtRegisteredClaimNames</c>.
/// </summary>
public static class JwtClaims
{
    /// <summary>
    /// Id of the session row backing this token. Logout revokes by this value.
    /// </summary>
    public const string SessionId = "sid";

    public const string ProfilePictureKey = "picture_key";

    public const string Tenant = "tenant";

    /// <summary>
    /// App the account logged into. Also mirrored in <c>aud</c>.
    /// </summary>
    public const string AppLogged = "app";

    /// <summary>
    /// Every app the account belongs to.
    /// </summary>
    public const string Apps = "apps";

    public const string Roles = "roles";
}

public sealed record TenantClaim
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("mini_logo_key")]
    public string? MiniLogoKey { get; init; }

    [JsonPropertyName("cover_logo_key")]
    public string? CoverLogoKey { get; init; }

    [JsonPropertyName("web_url")]
    public string? WebUrl { get; init; }
}

public sealed record AppClaim
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

public sealed record RoleClaim
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("key")]
    public string? Key { get; init; }

    [JsonPropertyName("app_id")]
    public Guid AppId { get; init; }

    [JsonPropertyName("permissions")]
    public List<PermissionClaim> Permissions { get; init; } = [];
}

public sealed record PermissionClaim
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("key")]
    public string? Key { get; init; }
}
