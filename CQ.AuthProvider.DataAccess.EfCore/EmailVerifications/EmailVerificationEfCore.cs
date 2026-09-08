using CQ.AuthProvider.BusinessLogic.EmailVerifications;

namespace CQ.AuthProvider.DataAccess.EfCore.EmailVerifications;

public sealed record class EmailVerificationEfCore()
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Email { get; init; } = null!;

    public string Token { get; set; } = Guid.NewGuid().ToString("N");

    public int Code { get; set; }

    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(EmailVerification.TOLERANCE_IN_MINUTES);

    // For new EmailVerification
    public EmailVerificationEfCore(
        Guid id,
        string email,
        string token,
        int code)
        : this()
    {
        Id = id;
        Email = email;
        Token = token;
        Code = code;
    }
}
