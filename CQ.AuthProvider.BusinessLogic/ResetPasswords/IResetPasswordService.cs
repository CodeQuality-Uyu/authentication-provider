namespace CQ.AuthProvider.BusinessLogic.ResetPasswords;

public interface IResetPasswordService
{
    Task CreateAsync(CreateResetPasswordArgs args);

    Task VerifyAsync(VerifyResetPasswordArgs args);

    Task AcceptAsync(AcceptResetPasswordArgs args);
}
