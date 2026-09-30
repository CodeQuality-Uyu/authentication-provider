using CQ.AuthProvider.DataAccess.EfCore.Accounts;

namespace CQ.AuthProvider.DataAccess.EfCore.Tenants;

public sealed record class TenantEfCore()
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Name { get; set; } = null!;

    public string? WebUrl { get; set; }

    public List<AccountEfCore> Accounts { get; init; } = [];
}
