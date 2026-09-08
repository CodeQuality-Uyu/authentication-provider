using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Roles;
using CQ.AuthProvider.BusinessLogic.Sessions;
using CQ.UnitOfWork.Abstractions;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.GoogleAuth;

internal sealed class GoogleAuthService(
    IGoogleTokenValidator googleTokenValidator,
    IGoogleIdentityRepository googleIdentityRepository,
    IAccountRepository accountRepository,
    IAppService appService,
    IRoleRepository roleRepository,
    ISessionInternalService sessionService,
    IAccountDataEnricher accountDataEnricher,
    IUnitOfWork unitOfWork)
    : IGoogleAuthService
{
    public async Task<Session> CreateSessionAsync(CreateSessionGoogleArgs args)
    {
        var app = await appService
            .GetByIdAsync(args.AppId)
            .ConfigureAwait(false);

        if (Guard.IsNullOrEmpty(app.GoogleClientId))
        {
            throw new InvalidOperationException($"App ({app.Id}) doesn't have Google Sign-In configured");
        }

        var profile = await googleTokenValidator
            .ValidateAsync(args.IdToken, app.GoogleClientId!)
            .ConfigureAwait(false);

        if (!profile.EmailVerified || Guard.IsNullOrEmpty(profile.Email))
        {
            throw new InvalidOperationException("Google account must have a verified email");
        }

        var googleIdentity = await googleIdentityRepository
            .GetByGoogleSubAsync(profile.Sub)
            .ConfigureAwait(false);

        Account account;
        App appOfAccount;

        if (googleIdentity is not null)
        {
            account = await accountRepository
                .GetByIdAsync(googleIdentity.AccountId, args.AppId)
                .ConfigureAwait(false);

            appOfAccount = GetAppOrThrow(account, args.AppId);
        }
        else
        {
            var emailExists = await accountRepository
                .ExistByEmailAsync(profile.Email)
                .ConfigureAwait(false);

            if (emailExists)
            {
                var existingAccount = await accountRepository
                    .GetByEmailAsync(profile.Email)
                    .ConfigureAwait(false);

                account = await accountRepository
                    .GetByIdAsync(existingAccount.Id, args.AppId)
                    .ConfigureAwait(false);

                appOfAccount = GetAppOrThrow(account, args.AppId);
            }
            else
            {
                var role = await roleRepository
                    .GetDefaultByTenantIdAsync(null, app.Id, app.Tenant.Id)
                    .ConfigureAwait(false);

                account = Account.New(
                    profile.Email,
                    Guard.IsNotNullOrEmpty(profile.GivenName) ? profile.GivenName! : profile.Email,
                    profile.FamilyName ?? string.Empty,
                    null,
                    profile.Locale ?? "en",
                    "UTC",
                    role,
                    app)
                    // Ya se validó arriba que Google confirma el email (profile.EmailVerified):
                    // no hace falta un segundo paso de verificación, se asume verificada de una.
                    with
                    { IsEmailVerified = true };

                await accountRepository
                    .CreateAsync(account)
                    .ConfigureAwait(false);

                appOfAccount = app;
            }

            await googleIdentityRepository
                .CreateAndSaveAsync(new GoogleIdentity
                {
                    AccountId = account.Id,
                    GoogleSub = profile.Sub
                })
                .ConfigureAwait(false);
        }

        var session = await sessionService
            .CreateAsync(account, appOfAccount)
            .ConfigureAwait(false);

        await unitOfWork
            .CommitChangesAsync()
            .ConfigureAwait(false);

        var appData = await accountDataEnricher
            .GetAsync(appOfAccount, session.Token)
            .ConfigureAwait(false);

        return session with { AppData = appData };
    }

    private static App GetAppOrThrow(Account account, Guid appId)
    {
        var app = account.Apps.FirstOrDefault(a => a.Id == appId);

        if (Guard.IsNull(app))
        {
            throw new InvalidOperationException($"Account ({account.Email}) doesn't exist in app ({appId})");
        }

        return app!;
    }
}
