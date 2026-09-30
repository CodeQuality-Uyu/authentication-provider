namespace CQ.AuthProvider.WebApi.Controllers.Tenants;

public sealed record TenantBasicInfoResponse(
    Guid Id,
    string Name,
    OwnerTenantBasicInfoResponse Owner);

public readonly struct TenantOfAccountBasicInfoResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public string? WebUrl { get; init; }
}

public sealed record OwnerTenantBasicInfoResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email);