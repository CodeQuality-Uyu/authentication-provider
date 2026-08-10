namespace CQ.AuthProvider.WebApi.Controllers.Accounts;

public sealed record AccountDetailResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? ProfilePictureKey,
    bool IsEmailVerified,
    List<AccountRoleDetailResponse> Roles);

public sealed record AccountRoleDetailResponse(
    Guid Id,
    string Name,
    string? Key,
    string Description,
    bool IsPublic,
    List<AccountPermissionResponse> Permissions);

public sealed record AccountPermissionResponse(
    Guid Id,
    string Name,
    string Key,
    string Description);
