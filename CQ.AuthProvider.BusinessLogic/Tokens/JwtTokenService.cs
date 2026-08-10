using System.Text.Json;
using CQ.ApiElements;
using CQ.AuthProvider.BusinessLogic.AppConfig;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Permissions;
using CQ.AuthProvider.BusinessLogic.Roles;
using CQ.AuthProvider.BusinessLogic.Tenants;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CQ.AuthProvider.BusinessLogic.Tokens;

/// <summary>
/// Issues and validates the access token as a self contained RS256 JWT: no
/// database round trip on validation, which is what lets the apps validate it
/// on their own against /.well-known/jwks.json.
/// </summary>
/// <remarks>
/// Being self contained, an access token stays valid until it expires even
/// after logout. Revocation happens on the refresh token, which is why the
/// access token is short lived. See docs/jwt-sessions.md.
/// </remarks>
internal sealed class JwtTokenService(
    IJwtKeyProvider keyProvider,
    IOptions<JwtSection> options)
    : ITokenService
{
    private static readonly JsonWebTokenHandler _handler = new();

    private readonly JwtSection _jwt = options.Value;

    public string AuthorizationTypeHandled => "Bearer";

    public Task<string> CreateAsync(object item)
    {
        if (item is not SessionTokenPayload payload)
        {
            throw new InvalidOperationException(
                $"{nameof(JwtTokenService)} expects a {nameof(SessionTokenPayload)} but got '{item?.GetType().Name ?? "null"}'");
        }

        var now = DateTime.UtcNow;

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwt.Issuer,
            Audience = payload.App.Id.ToString(),
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(_jwt.AccessTokenExpirationInMinutes),
            SigningCredentials = keyProvider.SigningCredentials,
            Claims = BuildClaims(payload)
        };

        return Task.FromResult(_handler.CreateToken(descriptor));
    }

    public Task<bool> IsValidAsync(string value)
    {
        return Task.FromResult(_handler.CanReadToken(value));
    }

    public async Task<object?> GetOrDefaultAsync(string value)
    {
        var result = await _handler
            .ValidateTokenAsync(value, BuildValidationParameters())
            .ConfigureAwait(false);

        if (!result.IsValid || result.SecurityToken is not JsonWebToken jwt)
        {
            return null;
        }

        var payload = ReadPayload(jwt);

        return payload == null
            ? null
            : BuildAccountLogged(payload, value);
    }

    private TokenValidationParameters BuildValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwt.Issuer,

            // The audience is the id of the app the account logged into, which
            // is not a fixed set here. Each app checks it matches its own id.
            ValidateAudience = false,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(_jwt.ClockSkewInSeconds),

            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keyProvider.ValidationKeys,
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256]
        };
    }

    #region Write
    /// <summary>
    /// Nested claims are built out of dictionaries and lists on purpose: those
    /// are the shapes every version of the token handler serializes verbatim.
    /// </summary>
    private static Dictionary<string, object> BuildClaims(SessionTokenPayload payload)
    {
        var account = payload.Account;

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = account.Id.ToString(),
            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            [JwtClaims.SessionId] = payload.SessionId.ToString(),
            [JwtClaims.AppLogged] = BuildApp(payload.App),
            [JwtClaims.Apps] = account.Apps.ConvertAll(BuildApp),
            [JwtClaims.Roles] = account.Roles.ConvertAll(BuildRole)
        };

        AddIfPresent(claims, JwtRegisteredClaimNames.Email, account.Email);
        AddIfPresent(claims, JwtRegisteredClaimNames.GivenName, account.FirstName);
        AddIfPresent(claims, JwtRegisteredClaimNames.FamilyName, account.LastName);
        AddIfPresent(claims, JwtRegisteredClaimNames.Name, account.FullName);
        AddIfPresent(claims, "locale", account.Locale);
        AddIfPresent(claims, "zoneinfo", account.TimeZone);
        AddIfPresent(claims, JwtClaims.ProfilePictureKey, account.ProfilePictureKey);

        if (account.Tenant != null)
        {
            claims[JwtClaims.Tenant] = BuildTenant(account.Tenant);
        }

        return claims;
    }

    private static void AddIfPresent(
        Dictionary<string, object> claims,
        string name,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            claims[name] = value;
        }
    }

    private static Dictionary<string, object> BuildTenant(Tenant tenant)
    {
        var claim = new Dictionary<string, object>
        {
            ["id"] = tenant.Id.ToString()
        };

        AddIfPresent(claim, "name", tenant.Name);
        AddIfPresent(claim, "mini_logo_key", tenant.MiniLogoKey);
        AddIfPresent(claim, "cover_logo_key", tenant.CoverLogoKey);
        AddIfPresent(claim, "web_url", tenant.WebUrl);

        return claim;
    }

    private static Dictionary<string, object> BuildApp(App app)
    {
        var claim = new Dictionary<string, object>
        {
            ["id"] = app.Id.ToString()
        };

        AddIfPresent(claim, "name", app.Name);

        return claim;
    }

    private static Dictionary<string, object> BuildRole(Role role)
    {
        var claim = new Dictionary<string, object>
        {
            ["id"] = role.Id.ToString(),
            ["permissions"] = role.Permissions.ConvertAll(BuildPermission)
        };

        AddIfPresent(claim, "name", role.Name);
        AddIfPresent(claim, "key", role.Key);

        // Role.AppId reads through Role.App, which is not always loaded.
        if (role.App != null)
        {
            claim["app_id"] = role.App.Id.ToString();
        }

        return claim;
    }

    private static Dictionary<string, object> BuildPermission(Permission permission)
    {
        var claim = new Dictionary<string, object>
        {
            ["id"] = permission.Id.ToString()
        };

        AddIfPresent(claim, "key", permission.Key);

        return claim;
    }
    #endregion Write

    #region Read
    /// <summary>
    /// The payload is deserialized straight from the encoded segment instead of
    /// going through the handler's claim accessors, which do not map nested
    /// objects consistently across versions.
    /// </summary>
    private static AccessTokenPayload? ReadPayload(JsonWebToken jwt)
    {
        try
        {
            var payload = Base64UrlEncoder.DecodeBytes(jwt.EncodedPayload);

            return JsonSerializer.Deserialize<AccessTokenPayload>(payload);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static AccountLogged BuildAccountLogged(
        AccessTokenPayload payload,
        string token)
    {
        var tenant = BuildTenant(payload.Tenant);

        return new AccountLogged
        {
            Id = payload.AccountId,
            Email = payload.Email!,
            FirstName = payload.FirstName!,
            LastName = payload.LastName!,
            FullName = payload.FullName!,
            ProfilePictureKey = payload.ProfilePictureKey,
            Locale = payload.Locale!,
            TimeZone = payload.TimeZone!,
            Tenant = tenant,
            Apps = payload.Apps.ConvertAll(a => BuildApp(a, tenant)),
            Roles = payload.Roles.ConvertAll(r => BuildRole(r, tenant)),
            AppLogged = BuildApp(payload.AppLogged, tenant),
            SessionId = payload.SessionId,
            Token = token
        };
    }

    private static Tenant BuildTenant(TenantClaim? claim)
    {
        return new Tenant
        {
            Id = claim?.Id ?? Guid.Empty,
            Name = claim?.Name!,
            MiniLogoKey = claim?.MiniLogoKey,
            CoverLogoKey = claim?.CoverLogoKey,
            WebUrl = claim?.WebUrl
        };
    }

    private static App BuildApp(
        AppClaim? claim,
        Tenant tenant)
    {
        return new App
        {
            Id = claim?.Id ?? Guid.Empty,
            Name = claim?.Name!,
            Tenant = tenant
        };
    }

    private static Role BuildRole(
        RoleClaim claim,
        Tenant tenant)
    {
        return new Role
        {
            Id = claim.Id,
            Name = claim.Name!,
            Key = claim.Key,
            Tenant = tenant,
            App = new App
            {
                Id = claim.AppId,
                Tenant = tenant
            },
            Permissions = claim.Permissions.ConvertAll(p => new Permission
            {
                Id = p.Id,
                Key = p.Key!
            })
        };
    }
    #endregion Read
}
