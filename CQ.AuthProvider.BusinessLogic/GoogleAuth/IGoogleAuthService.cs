using CQ.AuthProvider.BusinessLogic.Sessions;

namespace CQ.AuthProvider.BusinessLogic.GoogleAuth;

public interface IGoogleAuthService
{
    Task<Session> CreateSessionAsync(CreateSessionGoogleArgs args);
}
