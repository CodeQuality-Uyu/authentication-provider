using CQ.AuthProvider.BusinessLogic.Blobs;
using CQ.AuthProvider.WebApi.Controllers.Blobs;

namespace CQ.AuthProvider.WebApi.Controllers.Tenants;

// Tenía un Owner que el dominio ya no tiene (se quitó en RemoveOwnerOfTenantTable): AutoMapper no
// podía construir el record y GET /tenants no respondía.
public sealed record TenantBasicInfoResponse(
    Guid Id,
    string Name);

public readonly struct TenantOfAccountBasicInfoResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public BlobReadResponse? MiniLogo { get; init; }

    public BlobReadResponse? CoverLogo { get; init; }

    public string? WebUrl { get; init; }
}

public sealed record OwnerTenantBasicInfoResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email);
