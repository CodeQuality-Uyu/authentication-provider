using CQ.AuthProvider.BusinessLogic.AppConfig;
using CQ.AuthProvider.BusinessLogic.Emails.MailingApi;
using CQ.AuthProvider.BusinessLogic.Emails.Templates;
using Microsoft.Extensions.Options;

namespace CQ.AuthProvider.BusinessLogic.Emails;

/// <summary>
/// Composes the mail HTML (via <see cref="IEmailTemplateBuilder"/>) and hands the finished mail
/// off to the shared mailing-api for the actual send. This service owns the branding/content —
/// mailing-api is intentionally ignorant of it, it only dispatches whatever provider it's
/// configured for (Resend, etc.).
/// </summary>
internal sealed class EmailService(
    IMailingApiClient _mailingApiClient,
    IEmailTemplateBuilder _templateBuilder,
    IOptions<MailingSection> mailingOptions)
    : IEmailService
{
    private readonly MailingSection _mailing = mailingOptions.Value;

    public Task SendResetPasswordAsync(
        string to,
        int code,
        string? logoUrl) =>
        SendAsync(
            to,
            "Restablecé tu contraseña",
            _templateBuilder.BuildResetPassword(new BuildResetPasswordArgs(code, logoUrl)));

    public Task SendInviteUserAsync(
        string to,
        string creatorName,
        int code,
        string? logoUrl) =>
        SendAsync(
            to,
            "Te invitaron a unirte",
            _templateBuilder.BuildInviteUser(new BuildInviteUserArgs(creatorName, code, logoUrl)));

    public Task SendEmailVerificationAsync(
        string to,
        int code,
        string? verificationUrl,
        string? logoUrl) =>
        SendAsync(
            to,
            "Confirmá tu cuenta",
            _templateBuilder.BuildEmailVerification(new BuildEmailVerificationArgs(code, verificationUrl, logoUrl)));

    private Task SendAsync(
        string to,
        string subject,
        string html) =>
        _mailingApiClient.SendAsync(new SendMailArgs(
            From: $"{_mailing.FromName} <{_mailing.FromEmail}>",
            To: to,
            Subject: subject,
            Html: html));
}
