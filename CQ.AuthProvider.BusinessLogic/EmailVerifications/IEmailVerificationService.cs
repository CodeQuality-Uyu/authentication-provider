using CQ.AuthProvider.BusinessLogic.Accounts;

namespace CQ.AuthProvider.BusinessLogic.EmailVerifications;

public interface IEmailVerificationService
{
    Task CreateAsync(CreateEmailVerificationArgs args);

    Task AcceptAsync(
        Guid id,
        AcceptEmailVerificationArgs args);
}

internal interface IEmailVerificationInternalService
    : IEmailVerificationService
{
    Task CreateAsync(Account account);
}
