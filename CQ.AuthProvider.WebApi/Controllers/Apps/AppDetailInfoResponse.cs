using CQ.AuthProvider.BusinessLogic.Blobs;

namespace CQ.AuthProvider.WebApi.Controllers.Apps;

public readonly struct AppDetailInfoResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public LogoResponse Logo { get; init; }

    public BackgroundResponse? Background { get; init; }

    public FatherAppBasicInfoResponse? FatherApp { get; init; }

    public AccountDataSourceResponse? AccountDataSource { get; init; }

    /// <summary>
    /// Google OAuth Client ID to use when rendering "Sign in with Google" for
    /// this app. Null when the app doesn't have Google login enabled. Public
    /// by design: a Client ID isn't a secret, it's meant to be embedded in the
    /// frontend that renders the Google button.
    /// </summary>
    public string? GoogleClientId { get; init; }
}

public readonly struct AccountDataSourceResponse
{
    public string Host { get; init; }

    public string Endpoint { get; init; }
}

public readonly struct LogoResponse
{
    public BlobReadResponse Color { get; init; }

    public BlobReadResponse Light { get; init; }

    public BlobReadResponse Dark { get; init; }
}

public readonly struct BackgroundResponse()
{
    public BlobReadResponse? Image { get; init; }

    public IList<string> Colors { get; init; } = [];

    public string? Config { get; init; }
}
