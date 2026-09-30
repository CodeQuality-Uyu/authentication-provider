namespace CQ.AuthProvider.BusinessLogic.Tenants;

public sealed record CreateTenantArgs(
    string Name,
    string? WebUrl);
