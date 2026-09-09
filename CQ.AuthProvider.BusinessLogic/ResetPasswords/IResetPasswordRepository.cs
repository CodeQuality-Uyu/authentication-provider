namespace CQ.AuthProvider.BusinessLogic.ResetPasswords;

public interface IResetPasswordRepository
{
    Task<ResetPassword> GetActiveForAcceptanceAsync(
        string email,
        int code);

    Task<ResetPassword?> GetOrDefaultByEmailAsync(string email);

    Task CreateAndSaveAsync(ResetPassword resetPassword);

    Task DeleteByIdAsync(Guid id);

    Task UpdateCodeByIdAsync(
        Guid id,
        int code);
}
