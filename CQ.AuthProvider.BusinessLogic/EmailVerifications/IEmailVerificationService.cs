using CQ.AuthProvider.BusinessLogic.Accounts;

namespace CQ.AuthProvider.BusinessLogic.EmailVerifications;

public interface IEmailVerificationService
{
    Task CreateAsync(CreateEmailVerificationArgs args);

    Task AcceptAsync(AcceptEmailVerificationArgs args);

    /// <summary>
    /// Se llama cuando ya se sabe que la cuenta no está verificada (login, registro) y hay que
    /// decidir si corresponde reenviar. Si el código/token pendiente ya venció (o no existe),
    /// genera y manda uno nuevo y devuelve <see langword="true"/>; si el que había sigue vigente,
    /// no hace nada y devuelve <see langword="false"/>.
    /// </summary>
    Task<bool> EnsureVerificationSentAsync(Account account);
}

internal interface IEmailVerificationInternalService
    : IEmailVerificationService
{
    /// <summary>
    /// Paso 3 del registro: confirma que el email ya se verificó (paso 2) y consume ese
    /// registro (lo borra), para que no se pueda reusar. Tira si no hay una verificación
    /// vigente y marcada como verificada para este email + código/token.
    /// </summary>
    Task ConsumeVerifiedAsync(
        string email,
        string? token,
        int? code);
}
