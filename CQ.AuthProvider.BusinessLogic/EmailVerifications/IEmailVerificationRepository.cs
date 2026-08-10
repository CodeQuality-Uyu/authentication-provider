namespace CQ.AuthProvider.BusinessLogic.EmailVerifications;

public interface IEmailVerificationRepository
{
    Task<EmailVerification> GetActiveForAcceptanceAsync(
        Guid id,
        string email,
        string? token,
        int? code);

    Task<EmailVerification?> GetOrDefaultByEmailAsync(string email);

    Task CreateAndSaveAsync(EmailVerification emailVerification);

    Task DeleteByIdAsync(Guid id);

    Task UpdateByIdAsync(
        Guid id,
        string token,
        int code);
}
