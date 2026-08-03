namespace CQ.AuthProvider.BusinessLogic.GoogleAuth;

public interface IGoogleIdentityRepository
{
    Task CreateAndSaveAsync(GoogleIdentity identity);

    Task<GoogleIdentity?> GetByGoogleSubAsync(string googleSub);
}
