namespace CQ.AuthProvider.BusinessLogic.Emails
{
    internal interface IEmailService
    {
        Task SendResetPasswordAsync(
            string to,
            int code,
            string? logoUrl);

        Task SendInviteUserAsync(
            string to,
            string creatorName,
            int code,
            string? logoUrl);

        Task SendEmailVerificationAsync(
            string to,
            int code,
            string? verificationUrl,
            string? logoUrl);
    }
}
