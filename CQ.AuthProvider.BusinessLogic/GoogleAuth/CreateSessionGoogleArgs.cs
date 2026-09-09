namespace CQ.AuthProvider.BusinessLogic.GoogleAuth;

public sealed record CreateSessionGoogleArgs(
    string IdToken,
    Guid AppId);
