using CQ.Blobs;

namespace CQ.AuthProvider.WebApi.Controllers.Apps;

public readonly struct AppDetailInfoResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public LogoResponse Logo { get; init; }

    public FatherAppBasicInfoResponse? FatherApp { get; init; }

    public AccountDataSourceResponse? AccountDataSource { get; init; }

    /// <summary>
    /// Google OAuth Client ID to use when rendering "Sign in with Google" for
    /// this app. Null when the app doesn't have Google login enabled. Public
    /// by design: a Client ID isn't a secret, it's meant to be embedded in the
    /// frontend that renders the Google button.
    /// </summary>
    public string? GoogleClientId { get; init; }

    /// <summary>
    /// Whether signing up in this app goes through email verification. When
    /// false the frontend can skip the "request code / enter code" screens and
    /// post straight to account creation.
    /// </summary>
    public bool RequiresEmailVerification { get; init; }
}

public readonly struct AccountDataSourceResponse
{
    public string Host { get; init; }

    public string Endpoint { get; init; }
}

public readonly struct LogoResponse
{
    public BlobRead Color { get; init; }

    public BlobRead Light { get; init; }

    public BlobRead Dark { get; init; }
}
