namespace CQ.AuthProvider.BusinessLogic.Apps;

public sealed record CreateAppArgs(
    string Name,
    bool IsDefault,
    Logo Logo,
    Background? Background,
    bool RegisterToIt = false,
    AccountDataSource? AccountDataSource = null,
    string? GoogleClientId = null);

public sealed record CreateClientAppArgs(
    string Name,
    Logo? Logo,
    Background? Background,
    AccountDataSource? AccountDataSource = null,
    string? GoogleClientId = null);