namespace CQ.AuthProvider.WebApi.Controllers.Apps;

public sealed record AppDetailInfoResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public Logo Logo { get; init; } = null!;

    public Background Background { get; init; } = null!;
}

public sealed record Background
{
    public string? ColorConfig { get; init; }
    public string? ColorKey { get; init; }
}

public sealed record Logo
{
    public string ColorKey { get; init; } = null!;
    public string WhiteKey { get; init; } = null!;
    public string BlackKey { get; init; } = null!;
}