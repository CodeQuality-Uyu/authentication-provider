using CQ.AuthProvider.BusinessLogic.EmailVerifications;
using CQ.AuthProvider.DataAccess.EfCore.Accounts;

namespace CQ.AuthProvider.DataAccess.EfCore.EmailVerifications;

public sealed record class EmailVerificationEfCore()
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid AccountId { get; init; }

    public AccountEfCore Account { get; init; } = null!;

    public string Token { get; set; } = Guid.NewGuid().ToString("N");

    public int Code { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; init; } = DateTime.UtcNow.AddMinutes(EmailVerification.TOLERANCE_IN_MINUTES);

    // For new EmailVerification
    public EmailVerificationEfCore(
        Guid id,
        string token,
        int code,
        Guid accountId)
        : this()
    {
        Id = id;
        Token = token;
        Code = code;
        AccountId = accountId;
    }
}
