using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.AuthProvider.BusinessLogic.Tokens;

namespace CQ.AuthProvider.BusinessLogic.Accounts;

public sealed record CreateAccountResult(
    Guid Id,
    string Email,
    string FullName,
    string FirstName,
    string LastName,
    string? ProfilePictureKey,
    string Locale,
    string TimeZone,
    App AppLogged,
    string Token,
    TokenFormat TokenFormat,
    DateTime? TokenExpiresAt,
    string? RefreshToken,
    List<string> Roles,
    List<string> Permissions,
    Tenant Tenant);
