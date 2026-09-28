namespace CQ.AuthProvider.BusinessLogic.Sessions;

public interface ISessionRepository
{
    Task CreateAsync(Session session);

    Task<Session> GetByTokenAsync(string token);

    Task DeleteByTokenAsync(string token);

    Task DeleteAndSaveByAccountIdAndAppIdAsync(
        Guid accountId,
        Guid appId);
}
