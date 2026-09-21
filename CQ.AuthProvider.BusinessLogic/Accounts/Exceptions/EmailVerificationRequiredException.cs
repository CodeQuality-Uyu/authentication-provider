namespace CQ.AuthProvider.BusinessLogic.Accounts.Exceptions;

/// <summary>
/// The app requires a verified email to sign up (<see cref="Apps.App.RequiresEmailVerification"/>)
/// but the request didn't carry any proof of it. Before this check existed the rule lived in
/// <c>CreateAccountArgsValidator</c> and came back as a plain validation error; it moved into the
/// service because whether it applies depends on the app, which the validator can't read.
/// </summary>
public sealed class EmailVerificationRequiredException(
    string email,
    Guid appId)
    : Exception
{
    public string Email { get; } = email;

    public Guid AppId { get; } = appId;
}
