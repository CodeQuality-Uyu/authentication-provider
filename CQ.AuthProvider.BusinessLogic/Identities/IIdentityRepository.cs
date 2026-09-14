namespace CQ.AuthProvider.BusinessLogic.Identities;

public interface IIdentityRepository
{
    Task CreateAndSaveAsync(Identity identity, bool passwordIsHash = false);

    Task UpdatePasswordByIdAsync(
        Guid id,
        string oldPassword,
        string newPassword);

    // A diferencia de UpdatePasswordByIdAsync, no exige contraseña vieja: la usa el flujo de
    // "olvidé mi contraseña" (verificado por email/código), donde puede no existir un Identity
    // todavía (p. ej. cuentas creadas solo por Google Sign-In) y hay que crearlo recién ahí.
    Task SetPasswordByIdAsync(
        Guid id,
        string email,
        string newPassword);

    Task DeleteAndSaveByIdAsync(Guid id);

    Task<Identity> GetByCredentialsAsync(
        string email,
        string password);
}
