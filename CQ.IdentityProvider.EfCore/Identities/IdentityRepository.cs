using CQ.AuthProvider.BusinessLogic.Identities;
using CQ.Exceptions;
using CQ.UnitOfWork.EfCore.Core;
using Microsoft.AspNetCore.Identity;

namespace CQ.IdentityProvider.EfCore.Identities;

public sealed class IdentityRepository(
    PasswordHasher<string> passwordHasher,
    IdentityDbContext context)
    : EfCoreRepository<Identity>(context),
    IIdentityRepository
{
    public async Task DeleteAndSaveByIdAsync(Guid id)
    {
        await DeleteAndSaveAsync(i => i.Id == id).ConfigureAwait(false);
    }

    public async Task<Identity> GetByCredentialsAsync(
        string email,
        string password)
    {
        var identity = await GetAsync(i => i.Email == email).ConfigureAwait(false);

        var result = passwordHasher.VerifyHashedPassword(identity.Id.ToString(), identity.Password, password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new SpecificResourceNotFoundException<Identity>("condition", string.Empty);
        }

        return identity;
    }

    public async Task UpdatePasswordByIdAsync(
        Guid id,
        string oldPassword,
        string newPassword)
    {
        var identity = await GetByIdAsync(id).ConfigureAwait(false);

        var result = passwordHasher.VerifyHashedPassword(identity.Id.ToString(), identity.Password, oldPassword);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new SpecificResourceNotFoundException<Identity>("id", string.Empty);
        }

        var passwordHashed = passwordHasher.HashPassword(id.ToString(), newPassword);

        identity.Password = passwordHashed;

        await BaseContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task SetPasswordByIdAsync(
        Guid id,
        string email,
        string newPassword)
    {
        var passwordHashed = passwordHasher.HashPassword(id.ToString(), newPassword);

        var identity = await GetOrDefaultAsync(i => i.Id == id).ConfigureAwait(false);

        if (identity is not null)
        {
            identity.Password = passwordHashed;

            await BaseContext.SaveChangesAsync().ConfigureAwait(false);

            return;
        }

        // Cuenta sin Identity todavía (p. ej. se creó solo con Google Sign-In): el reset de
        // contraseña le da una por primera vez en lugar de fallar con "Identity not found".
        var newIdentity = new Identity
        {
            Id = id,
            Email = email,
            Password = passwordHashed
        };

        await CreateAndSaveAsync(newIdentity).ConfigureAwait(false);
    }

    async Task IIdentityRepository.CreateAndSaveAsync(Identity identity, bool passwordIsHash)
    {
        identity.Password = passwordIsHash
            ? identity.Password
            : passwordHasher.HashPassword(identity.Id.ToString(), identity.Password);

        await CreateAndSaveAsync(identity).ConfigureAwait(false);
    }
}
