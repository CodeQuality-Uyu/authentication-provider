namespace CQ.AuthProvider.BusinessLogic.GoogleAuth;

/// <summary>
/// Validates a Google id_token signature and audience against Google's public
/// keys. Implemented outside this project (WebApi) to keep the Google SDK
/// dependency out of the domain layer.
/// </summary>
public interface IGoogleTokenValidator
{
    Task<GoogleProfile> ValidateAsync(
        string idToken,
        string clientId);
}
