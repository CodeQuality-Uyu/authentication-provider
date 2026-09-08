using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Emails;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.EmailVerifications;

internal sealed class EmailVerificationService(
    IEmailVerificationRepository _emailVerificationRepository,
    IAccountRepository _accountRepository,
    IAppService _appService,
    ITenantRepository _tenantRepository,
    IAccountEmailBrandingResolver _brandingResolver,
    IEmailService _emailService)
    : IEmailVerificationInternalService
{
    public async Task CreateAsync(CreateEmailVerificationArgs args)
    {
        // Paso 1 del registro: todavía no existe la cuenta. Si ya hay una con este email, no
        // tiene sentido "verificarlo" — el registro va a fallar igual más adelante.
        var emailInUse = await _accountRepository
            .ExistByEmailAsync(args.Email)
            .ConfigureAwait(false);

        if (emailInUse)
        {
            throw new InvalidOperationException($"Email ({args.Email}) is in use");
        }

        var app = await _appService
            .GetByIdAsync(args.AppId)
            .ConfigureAwait(false);

        var oldEmailVerification = await _emailVerificationRepository
            .GetOrDefaultByEmailAsync(args.Email)
            .ConfigureAwait(false);

        await CreateOrRefreshAndSendAsync(args.Email, app.Tenant.Id, oldEmailVerification)
            .ConfigureAwait(false);
    }

    public async Task<bool> EnsureVerificationSentAsync(Account account)
    {
        var existing = await _emailVerificationRepository
            .GetOrDefaultByEmailAsync(account.Email)
            .ConfigureAwait(false);

        // Ya hay uno vigente (no vencido): no se toca ni se reenvía nada.
        if (existing is not null && existing.ExpiresAt > DateTime.UtcNow)
        {
            return false;
        }

        await CreateOrRefreshAndSendAsync(account.Email, account.Tenant.Id, existing)
            .ConfigureAwait(false);

        return true;
    }

    private async Task CreateOrRefreshAndSendAsync(
        string email,
        Guid tenantId,
        EmailVerification? existing)
    {
        string token;
        int code;
        if (Guard.IsNull(existing))
        {
            var emailVerification = EmailVerification.New(email);

            token = emailVerification.Token;
            code = emailVerification.Code;

            await _emailVerificationRepository
                .CreateAndSaveAsync(emailVerification)
                .ConfigureAwait(false);
        }
        else
        {
            token = Guid.NewGuid().ToString("N");
            code = EmailVerification.NewCode();

            await _emailVerificationRepository
                .UpdateByIdAsync(
                existing.Id,
                token,
                code)
                .ConfigureAwait(false);
        }

        await SendEmailAsync(
            email,
            tenantId,
            token,
            code)
            .ConfigureAwait(false);
    }

    public async Task ConsumeVerifiedAsync(
        string email,
        string? token,
        int? code)
    {
        var emailVerification = await _emailVerificationRepository
            .GetVerifiedForConsumptionAsync(email, token, code)
            .ConfigureAwait(false);

        await _emailVerificationRepository
            .DeleteByIdAsync(emailVerification.Id)
            .ConfigureAwait(false);
    }

    public async Task AcceptAsync(AcceptEmailVerificationArgs args)
    {
        // Paso 2 del registro (o reverificación de una cuenta existente): solo prende
        // IsVerified, no borra el registro — el paso 3 (crear la cuenta) todavía lo necesita
        // para confirmar que este email realmente se probó antes de darle de alta.
        var emailVerification = await _emailVerificationRepository
            .GetActiveForAcceptanceAsync(
            args.Email,
            args.Token,
            args.Code)
            .ConfigureAwait(false);

        await _emailVerificationRepository
            .MarkAsVerifiedByIdAsync(emailVerification.Id)
            .ConfigureAwait(false);

        // Si ya existe una cuenta con este email (caso "reverificar", no "recién registrado"),
        // se refleja también ahí.
        var accountExists = await _accountRepository
            .ExistByEmailAsync(args.Email)
            .ConfigureAwait(false);

        if (accountExists)
        {
            var account = await _accountRepository
                .GetByEmailAsync(args.Email)
                .ConfigureAwait(false);

            await _accountRepository
                .UpdateEmailVerifiedByIdAsync(account.Id)
                .ConfigureAwait(false);
        }
    }

    private async Task SendEmailAsync(
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

        var verificationUrl = Guard.IsNullOrEmpty(tenant.WebUrl)
            ? null
            : $"{tenant.WebUrl!.TrimEnd('/')}/auth/verify-email?email={Uri.EscapeDataString(email)}&token={token}";

        await _emailService
            .SendEmailVerificationAsync(
            email,
            code,
            verificationUrl,
            logoUrl)
            .ConfigureAwait(false);
    }
}
