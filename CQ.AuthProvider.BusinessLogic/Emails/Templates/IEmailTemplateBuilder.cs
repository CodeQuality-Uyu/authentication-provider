namespace CQ.AuthProvider.BusinessLogic.Emails.Templates;

internal interface IEmailTemplateBuilder
{
    string BuildResetPassword(BuildResetPasswordArgs args);

    string BuildInviteUser(BuildInviteUserArgs args);

    string BuildEmailVerification(BuildEmailVerificationArgs args);
}

internal sealed record BuildResetPasswordArgs(
    int Code,
    string? LogoUrl);

internal sealed record BuildInviteUserArgs(
    string CreatorName,
    int Code,
    string? LogoUrl);

/// <param name="VerificationUrl">
/// Link that accepts the verification by <c>Token</c> (one click). Null when the tenant has no
/// <c>WebUrl</c> configured yet — the mail still works via the OTP code in that case.
/// </param>
internal sealed record BuildEmailVerificationArgs(
    int Code,
    string? VerificationUrl,
    string? LogoUrl);
