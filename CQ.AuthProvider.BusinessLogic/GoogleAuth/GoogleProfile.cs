namespace CQ.AuthProvider.BusinessLogic.GoogleAuth;

/// <summary>
/// Claims extracted from a Google id_token once its signature and audience have
/// been validated. <see cref="Sub"/> is Google's stable, never-reused account
/// identifier and is what we use to link a <see cref="GoogleIdentity"/> to an
/// <see cref="Accounts.Account"/>.
/// </summary>
public sealed record class GoogleProfile
{
    public required string Sub { get; init; }

    public string Email { get; init; } = string.Empty;

    public bool EmailVerified { get; init; }

    public string? Name { get; init; }

    public string? GivenName { get; init; }

    public string? FamilyName { get; init; }

    public string? Picture { get; init; }

    public string? Locale { get; init; }
}
