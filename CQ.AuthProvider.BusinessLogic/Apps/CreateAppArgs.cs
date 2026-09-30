namespace CQ.AuthProvider.BusinessLogic.Apps;

public sealed record CreateAppArgs(
    string Name,
    bool IsDefault,
    Logo Logo,
    bool RegisterToIt = false,
    AccountDataSource? AccountDataSource = null,
    string? GoogleClientId = null,
    bool RequiresEmailVerification = true);

public sealed record CreateClientAppArgs(
    string Name,
    Logo? Logo,
    AccountDataSource? AccountDataSource = null,
    string? GoogleClientId = null,
    bool RequiresEmailVerification = true);
