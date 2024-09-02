using CQ.AuthProvider.BusinessLogic.Abstractions.Apps;
using CQ.AuthProvider.BusinessLogic.Abstractions.Identities;
using CQ.AuthProvider.BusinessLogic.Abstractions.Roles;
using CQ.AuthProvider.BusinessLogic.Abstractions.Sessions;
using CQ.Exceptions;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.Abstractions.Accounts;

internal sealed class AccountService(
    IAccountRepository accountRepository,
    IIdentityRepository identityRepository,
    ISessionInternalService sessionService,
    IRoleInternalService roleInternalService,
    IAppInternalService appService
    )
    : IAccountInternalService
{
    #region Create
    public async Task<CreateAccountResult> InternalCreationAsync(CreateAccountArgs args)
    {
        var identity = await CreateIdentityAsync(args).ConfigureAwait(false);

        try
        {
            var role = await GetRoleAsync(args.RoleId).ConfigureAwait(false);
            var app = await appService
                .GetByIdAsync(args.AppId)
                .ConfigureAwait(false);

            var account = new Account(
                args.Email,
                args.FirstName,
                args.LastName,
                args.ProfilePictureId,
                args.Locale,
                args.TimeZone,
                role,
                app)
            {
                Id = identity.Id
            };

            await accountRepository
                .CreateAsync(account)
                .ConfigureAwait(false);

            var session = await sessionService
                .CreateAsync(
                identity,
                args.AppId)
                .ConfigureAwait(false);

            var result = new CreateAccountResult(
                account.Id,
                account.Email,
                account.FullName,
                account.FirstName,
                account.LastName,
                account.ProfilePictureId,
                account.Locale,
                account.TimeZone,
                session.Token,
                account.Roles.ConvertAll(r => r.Name));

            return result;
        }
        catch (SpecificResourceNotFoundException<Role>)
        {
            await identityRepository
                .DeleteByIdAsync(identity.Id)
                .ConfigureAwait(false);
            throw;
        }
    }

    public async Task<CreateAccountResult> CreateAsync(CreateAccountArgs args)
    {
        await AssertEmailInUseAsync(args.Email).ConfigureAwait(false);

        var result = await InternalCreationAsync(args).ConfigureAwait(false);

        return result;
    }

    private async Task AssertEmailInUseAsync(string email)
    {
        var existAuth = await accountRepository
            .ExistByEmailAsync(email)
            .ConfigureAwait(false);

        if (existAuth)
        {
            throw new SpecificResourceDuplicatedException<Account>(nameof(Account.Email), email);
        }
    }

    private async Task<Identity> CreateIdentityAsync(CreateAccountArgs newAccount)
    {
        var identity = new Identity(
            newAccount.Email,
            newAccount.Password);

        await identityRepository
            .CreateAsync(identity)
            .ConfigureAwait(false);

        return identity;
    }

    private async Task<Role> GetRoleAsync(string? roleId)
    {
        if (Guard.IsNull(roleId))
        {
            return await roleInternalService
                .GetDefaultAsync()
                .ConfigureAwait(false);
        }

        return await roleInternalService
            .GetByIdAsync(roleId)
            .ConfigureAwait(false);
    }
    #endregion

    public async Task UpdatePasswordAsync(
        string newPassword,
        Account userLogged)
    {
        Guard.ThrowIsInputInvalidPassword(newPassword, nameof(newPassword));

        await identityRepository
            .UpdatePasswordByIdAsync(
            userLogged.Id,
            newPassword)
            .ConfigureAwait(false);
    }

    public async Task<AccountLogged> GetByTokenAsync(string token)
    {
        var session = await sessionService
            .GetByTokenAsync(token)
            .ConfigureAwait(false);

        var account = await accountRepository
            .GetByIdAsync(session.Account.Id)
            .ConfigureAwait(false);

        return new AccountLogged(
            account,
            token,
            session.App);
    }

    public async Task<Account> GetByEmailAsync(string email)
    {
        return await accountRepository
            .GetByEmailAsync(email)
            .ConfigureAwait(false);
    }

    public async Task<Account> GetByIdAsync(string id)
    {
        return await accountRepository
            .GetByIdAsync(id)
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(
        UpdatePasswordArgs args,
        AccountLogged accountLogged)
    {
        await identityRepository
            .GetByCredentialsAsync(
            accountLogged.Email,
            args.OldPassword)
            .ConfigureAwait(false);

        await identityRepository
            .UpdatePasswordByIdAsync(
            accountLogged.Id,
            args.NewPassword)
            .ConfigureAwait(false);
    }
}
