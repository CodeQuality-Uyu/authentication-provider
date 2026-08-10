using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Emails;
using CQ.AuthProvider.BusinessLogic.EmailVerifications.Exceptions;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.EmailVerifications;

internal sealed class EmailVerificationService(
    IEmailVerificationRepository _emailVerificationRepository,
    IAccountRepository _accountRepository,
    ITenantRepository _tenantRepository,
    IAccountEmailBrandingResolver _brandingResolver,
    IEmailService _emailService)
    : IEmailVerificationInternalService
{
    public async Task CreateAsync(Account account)
    {
        var emailVerification = EmailVerification.New(account);

        await _emailVerificationRepository
            .CreateAndSaveAsync(emailVerification)
            .ConfigureAwait(false);

        await SendEmailAsync(
            emailVerification.Id,
            account.Email,
            account.Tenant.Id,
            emailVerification.Token,
            emailVerification.Code)
            .ConfigureAwait(false);
    }

    public async Task CreateAsync(CreateEmailVerificationArgs args)
    {
        var account = await _accountRepository
            .GetByEmailAsync(args.Email)
            .ConfigureAwait(false);

        if (account.IsEmailVerified)
        {
            throw new EmailAlreadyVerifiedException(account.Email);
        }

        var oldEmailVerification = await _emailVerificationRepository
            .GetOrDefaultByEmailAsync(args.Email)
            .ConfigureAwait(false);

        Guid id;
        string token;
        int code;
        if (Guard.IsNull(oldEmailVerification))
        {
            var emailVerification = EmailVerification.New(account);

            id = emailVerification.Id;
            token = emailVerification.Token;
            code = emailVerification.Code;

            await _emailVerificationRepository
                .CreateAndSaveAsync(emailVerification)
                .ConfigureAwait(false);
        }
        else
        {
            id = oldEmailVerification.Id;
            token = Guid.NewGuid().ToString("N");
            code = EmailVerification.NewCode();

            await _emailVerificationRepository
                .UpdateByIdAsync(
                id,
                token,
                code)
                .ConfigureAwait(false);
        }

        await SendEmailAsync(
            id,
            args.Email,
            account.Tenant.Id,
            token,
            code)
            .ConfigureAwait(false);
    }

    public async Task AcceptAsync(
        Guid id,
        AcceptEmailVerificationArgs args)
    {
        var emailVerification = await _emailVerificationRepository
            .GetActiveForAcceptanceAsync(
            id,
            args.Email,
            args.Token,
            args.Code)
            .ConfigureAwait(false);

        await _accountRepository
            .UpdateEmailVerifiedByIdAsync(emailVerification.Account.Id)
            .ConfigureAwait(false);

        await _emailVerificationRepository
            .DeleteByIdAsync(id)
            .ConfigureAwait(false);
    }

    private async Task SendEmailAsync(
        Guid id,
        string email,
        Guid tenantId,
        string token,
        int code)
    {
        var tenant = await _tenantRepository
            .GetByIdAsync(tenantId)
            .ConfigureAwait(false);

        var logoUrl = await _brandingResolver
            .GetLogoUrlAsync(tenantId)
            .ConfigureAwait(false);

        // TODO: confirmar con frontend la ruta real para aceptar por link (id + token) — esta es
        // una convención razonable, no una ruta ya existente en auth-provider-react-web/ecolors-react-web.
        var verificationUrl = Guard.IsNullOrEmpty(tenant.WebUrl)
            ? null
            : $"{tenant.WebUrl!.TrimEnd('/')}/verify-email?id={id}&token={token}";

        await _emailService
            .SendEmailVerificationAsync(
            email,
            code,
            verificationUrl,
            logoUrl)
            .ConfigureAwait(false);
    }
}
