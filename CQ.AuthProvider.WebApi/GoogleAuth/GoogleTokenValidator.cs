using CQ.AuthProvider.BusinessLogic.GoogleAuth;
using Google.Apis.Auth;

namespace CQ.AuthProvider.WebApi.GoogleAuth;

internal sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    public async Task<GoogleProfile> ValidateAsync(
        string idToken,
        string clientId)
    {
        GoogleJsonWebSignature.Payload payload;

        try
        {
            // Validates the id_token signature against Google's public keys (cached
            // internally by the library) and that its audience matches this app's
            // Client ID. No client_secret involved: Google itself vouches for the token.
            payload = await GoogleJsonWebSignature
                .ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [clientId]
                })
                .ConfigureAwait(false);
        }
        catch (InvalidJwtException ex)
        {
            throw new InvalidOperationException($"Invalid Google id_token: {ex.Message}", ex);
        }

        return new GoogleProfile
        {
            Sub = payload.Subject,
            Email = payload.Email ?? string.Empty,
            EmailVerified = payload.EmailVerified,
            Name = payload.Name,
            GivenName = payload.GivenName,
            FamilyName = payload.FamilyName,
            Picture = payload.Picture,
            Locale = payload.Locale
        };
    }
}
