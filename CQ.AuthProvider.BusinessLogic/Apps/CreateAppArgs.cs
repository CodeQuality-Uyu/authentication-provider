using Amazon.Runtime.SharedInterfaces;

namespace CQ.AuthProvider.BusinessLogic.Apps;

public sealed record CreateAppArgs(
    string Name,
    bool IsDefault,
    string CoverKey,
    CreateAppCoverBackgroundColorArgs? BackgroundColors,
    string? BackgroundCoverKey,
    bool RegisterToIt = false);

public sealed record CreateClientAppArgs(
    string Name,
    Logo Logo,
    Background Background);

public sealed record Background(
    string? ColorConfig,
    string? ColorKey);

public sealed record Logo(
    string ColorKey,
    string WhiteKey,
    string BlackKey);

public sealed record CreateAppCoverBackgroundColorArgs(
    List<string> Colors,
    string Config);