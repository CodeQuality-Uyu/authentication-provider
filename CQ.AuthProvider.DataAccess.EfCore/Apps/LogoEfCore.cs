namespace CQ.AuthProvider.DataAccess.EfCore.Apps;

public sealed record LogoEfCore()
{
    public string ColorKey { get; init; } = null!;
    public string WhiteKey { get; init; } = null!;
    public string BlackKey { get; init; } = null!;
}
