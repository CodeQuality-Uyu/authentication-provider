using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Emails;
using CQ.AuthProvider.BusinessLogic.Identities;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.ResetPasswords;

internal sealed class ResetPasswordService(
    IResetPasswordRepository _resetPasswordRepository,
    IIdentityRepository _identityRepository,
    IAccountRepository _accountRepository,
    IAccountEmailBrandingResolver _brandingResolver,
    IEmailService _emailService)
    : IResetPasswordService
{
    public async Task CreateAsync(CreateResetPasswordArgs args)
    {
        // Fetched unconditionally (not just on the "no pending reset" branch) — the tenant it
        // carries is needed either way to brand the mail.
        var account = await _accountRepository
            .GetByEmailAsync(args.Email)
            .ConfigureAwait(false);

        var oldResetPassword = await _resetPasswordRepository
            .GetOrDefaultByEmailAsync(args.Email)
            .ConfigureAwait(false);

        int code;
        if (Guard.IsNull(oldResetPassword))
        {
            var resetPassword = ResetPassword.New(account);

            code = resetPassword.Code;

            await _resetPasswordRepository
                .CreateAndSaveAsync(resetPassword)
                .ConfigureAwait(false);
        }
        else
        {
            code = ResetPassword.NewCode();

            await _resetPasswordRepository
                .UpdateCodeByIdAsync(
                oldResetPassword.Id,
                code)
                .ConfigureAwait(false);
        }

        var logoUrl = await _brandingResolver
            .GetLogoUrlAsync(account.Tenant.Id)
            .ConfigureAwait(false);

        await _emailService
            .SendResetPasswordAsync(
            args.Email,
            code,
            logoUrl)
            .ConfigureAwait(false);
    }

    public async Task AcceptAsync(
        Guid id,
        AcceptResetPasswordArgs args)
    {
        var resetPasswordOldApplication = await _resetPasswordRepository
            .GetActiveForAcceptanceAsync(
            id,
            args.Email,
            args.Code)
            .ConfigureAwait(false);

        await _identityRepository
            .UpdatePasswordByIdAsync(
            resetPasswordOldApplication.Account.Id,
            string.Empty,
            args.NewPassword)
            .ConfigureAwait(false);

        await _resetPasswordRepository
            .DeleteByIdAsync(id)
            .ConfigureAwait(false);
    }
}
