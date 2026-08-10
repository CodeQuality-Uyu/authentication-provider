using CQ.AuthProvider.BusinessLogic.Accounts;

namespace CQ.AuthProvider.BusinessLogic.EmailVerifications;

public sealed record class EmailVerification()
{
    public const int TOLERANCE_IN_MINUTES = 30; // a definir con negocio

    public Guid Id { get; init; } = Guid.NewGuid();

    public Account Account { get; init; } = null!;

    public string Token { get; init; } = Guid.NewGuid().ToString("N");

    public int Code { get; init; } = NewCode();

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; init; } = DateTime.UtcNow.AddMinutes(TOLERANCE_IN_MINUTES);

    public static EmailVerification New(Account account) => new()
    {
        Account = account
    };

    public static int NewCode()
    {
        return new Random()
            .Next(100000, 999999);
    }
}
