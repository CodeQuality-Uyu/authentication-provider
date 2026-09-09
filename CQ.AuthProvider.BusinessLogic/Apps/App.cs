using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.Apps;

public sealed record class App()
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; init; } = null!;

    public bool IsDefault { get; set; }

    public Logo Logo { get; init; } = null!;

    public Background? Background { get; init; }

    public Tenant Tenant { get; init; } = null!;

    public App? FatherApp { get; init; } = null!;

    /// <summary>
    /// Optional source the auth provider calls right after login to fetch
    /// app-specific data for the account (e.g. the teams a user belongs to in a
    /// sports app). The blob it returns is forwarded opaquely in the login
    /// response; the auth provider never interprets its shape.
    /// </summary>
    public AccountDataSource? AccountDataSource { get; init; }

    /// <summary>
    /// Google OAuth Client ID used to validate Google Sign-In id_tokens for this
    /// app. Null disables Google login for the app; the password/credentials
    /// login is unaffected either way.
    /// </summary>
    public string? GoogleClientId { get; init; }

    public App(
        string name,
        bool isDefault,
        Logo logo,
        Background? background,
        Tenant tenant,
        App? fatherApp,
        AccountDataSource? accountDataSource = null,
        string? googleClientId = null)
        : this()
    {
        Name = Guard.Normalize(name);
        IsDefault = isDefault;
        Tenant = tenant;
        Logo = logo;
        Background = background;
        FatherApp = fatherApp;
        AccountDataSource = accountDataSource;
        GoogleClientId = googleClientId;
    }
}

public sealed record AccountDataSource
{
    public string Host { get; set; } = null!;

    public string Endpoint { get; set; } = null!;
}


public sealed record Logo
{
    public string ColorKey { get; set; } = null!;

    public string LightKey { get; set; } = null!;
    
    public string DarkKey { get; set; } = null!;
}

public sealed record Background
{
    public IList<string> Colors { get; set; } = [];
    
    public string? Config { get; set; }

    public string? BackgroundKey { get; set; }
}
