namespace CQ.AuthProvider.BusinessLogic.EmailVerifications;

public interface IEmailVerificationRepository
{
    /// <summary>Paso 2 (aceptar): activo y vigente, sin importar si ya se había verificado.</summary>
    Task<EmailVerification> GetActiveForAcceptanceAsync(
        string email,
        string? token,
        int? code);

    /// <summary>Paso 3 (registro): activo, vigente, y ya marcado <see cref="EmailVerification.IsVerified"/>.</summary>
    Task<EmailVerification> GetVerifiedForConsumptionAsync(
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

    Task MarkAsVerifiedByIdAsync(Guid id);
}
