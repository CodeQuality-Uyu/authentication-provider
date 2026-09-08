namespace CQ.AuthProvider.BusinessLogic.EmailVerifications;

/// <summary>
/// Prueba de que alguien controla una casilla de email — no está atada a ninguna <c>Account</c>
/// porque el caso principal es justamente verificar el email ANTES de crear la cuenta (flujo de
/// registro: 1. pedir email y mandar el código, 2. validar el código, 3. recién ahí pedir el
/// resto de los datos y crear la cuenta ya verificada). <see cref="IsVerified"/> se prende en el
/// paso 2 y el registro (paso 3) la vuelve a chequear antes de crear la cuenta; recién ahí se
/// borra el registro.
/// </summary>
public sealed record class EmailVerification()
{
    public const int TOLERANCE_IN_MINUTES = 30; // a definir con negocio

    public Guid Id { get; init; } = Guid.NewGuid();

    public string Email { get; init; } = null!;

    public string Token { get; init; } = Guid.NewGuid().ToString("N");

    public int Code { get; init; } = NewCode();

    public bool IsVerified { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; init; } = DateTime.UtcNow.AddMinutes(TOLERANCE_IN_MINUTES);

    public static EmailVerification New(string email) => new()
    {
        Email = email
    };

    public static int NewCode()
    {
        return new Random()
            .Next(100000, 999999);
    }
}
