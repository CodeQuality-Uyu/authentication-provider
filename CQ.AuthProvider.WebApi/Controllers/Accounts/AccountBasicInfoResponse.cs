namespace CQ.AuthProvider.WebApi.Controllers.Accounts;

public readonly struct AccountBasicInfoResponse
{
    public Guid Id { get; init; }

    public string FirstName { get; init; }

    public string LastName { get; init; }

    public string FullName { get; init; }

    public string Email { get; init; }

    public string? ProfilePictureKey { get; init; }

    public List<AccountRoleResponse> Roles { get; init; }

    public List<AccountAppResponse> Apps { get; init; }

    public AccountTenantResponse Tenant { get; init; }
}

public sealed record AccountRoleResponse(
    Guid Id,
    string Name);

public sealed record AccountAppResponse(
    Guid Id,
    string Name);

public sealed record AccountTenantResponse(
    Guid Id,
    string Name);
