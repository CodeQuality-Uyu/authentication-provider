namespace CQ.AuthProvider.BusinessLogic.Apps;

/// <summary>
/// Updates the editable fields of an app: its name, its optional
/// <see cref="AccountDataSource"/> (null clears the source, disabling login
/// enrichment) and its optional Google OAuth Client ID (null disables Google
/// Sign-In for the app).
/// </summary>
public sealed record UpdateAppArgs(
    string Name,
    AccountDataSource? AccountDataSource,
    string? GoogleClientId = null);
