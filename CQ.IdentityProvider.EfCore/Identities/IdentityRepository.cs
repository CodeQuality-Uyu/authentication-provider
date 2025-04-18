﻿using CQ.AuthProvider.BusinessLogic.Identities;
using CQ.UnitOfWork.EfCore.Core;

namespace CQ.IdentityProvider.EfCore.Identities;

public sealed class IdentityRepository(
    IdentityDbContext context)
    : EfCoreRepository<Identity>(context),
    IIdentityRepository
{
    public async Task DeleteByIdAsync(Guid id)
    {
        await DeleteAndSaveAsync(i => i.Id == id).ConfigureAwait(false);
    }

    public async Task<Identity> GetByCredentialsAsync(
        string email,
        string password)
    {
        var identity = await GetAsync(i => i.Email == email && i.Password == password).ConfigureAwait(false);

        return identity;
    }

    public async Task UpdatePasswordByIdAsync(
        Guid id,
        string newPassword)
    {
        await UpdateAndSaveByIdAsync(id, new { Password = newPassword }).ConfigureAwait(false);
    }

    async Task IIdentityRepository.CreateAndSaveAsync(Identity identity)
    {
        await CreateAndSaveAsync(identity).ConfigureAwait(false);
    }
}
