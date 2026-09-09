using CQ.AuthProvider.BusinessLogic.Blobs;
using CQ.AuthProvider.WebApi.Controllers.Tenants;

namespace CQ.AuthProvider.WebApi.Controllers.Sessions;

public readonly struct SessionCreatedResponse
{
    public Guid Id { get; init; }

    public BlobReadResponse? ProfilePicture { get; init; }

    public string Email { get; init; }

    public string FirstName { get; init; }

    public string LastName { get; init; }

    public string FullName { get; init; }

    public string Token { get; init; }

    public bool IsEmailVerified { get; init; }

    public List<string> Roles { get; init; }

    public List<string> Permissions { get; init; }

    public SessionAppLoggedResponse AppLogged { get; init; }

    public TenantOfAccountBasicInfoResponse Tenant { get; init; }

    /// <summary>
    /// App-specific data fetched from the logged app's own API at login time.
    /// Null when the app registered no data source or the fetch failed. Its
    /// shape is defined entirely by the app, not by this provider.
    /// </summary>
    public object? AppData { get; init; }
}

public sealed record SessionAppLoggedResponse(
    Guid Id,
    string Name);