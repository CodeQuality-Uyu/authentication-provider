using CQ.ApiElements;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.EmailVerifications;
using CQ.AuthProvider.BusinessLogic.Identities;
using CQ.AuthProvider.BusinessLogic.Sessions.Exceptions;
using CQ.UnitOfWork.Abstractions;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.Sessions;

public sealed class SessionService(
    ISessionRepository sessionRepository,
    IIdentityRepository identityRepository,
    IAccountRepository accountRepository,
    ITokenService tokenService,
    IAccountDataEnricher accountDataEnricher,
    IEmailVerificationService emailVerificationService,
    IUnitOfWork _unitOfWork)
    : ISessionInternalService
{
    public async Task<Session> CreateAsync(CreateSessionCredentialsArgs args)
    {
        var identity = await identityRepository
            .GetByCredentialsAsync(args.Email, args.Password)
            .ConfigureAwait(false);

        var account = await accountRepository
            .GetByIdAsync(identity.Id, args.AppId)
            .ConfigureAwait(true);

        if (!account.IsEmailVerified)
        {
            // Si el código/token pendiente ya venció, se manda uno nuevo acá mismo — el usuario
            // no tiene por qué pedirlo aparte para enterarse de que puede reintentar.
            var verificationResent = await emailVerificationService
                .EnsureVerificationSentAsync(account)
                .ConfigureAwait(false);

            throw new EmailNotVerifiedException(account.Email, verificationResent);
        }

        var app = account
            .Apps
            .FirstOrDefault(a => a.Id == args.AppId);

        if (Guard.IsNull(app))
        {
            throw new InvalidOperationException($"Account ({account.Email}) doesn't exist in app ({args.AppId})");
        }

        var session = await CreateAsync(account, app).ConfigureAwait(false);

        await _unitOfWork
            .CommitChangesAsync()
            .ConfigureAwait(false);

        // Enrich after commit: the app's data endpoint validates the token by
        // calling back into this provider, so the session must already be persisted.
        var appData = await accountDataEnricher
            .GetAsync(app, session.Token)
            .ConfigureAwait(false);

        return session with { AppData = appData };
    }

    public async Task<Session> CreateAsync(
        Account account,
        App app)
    {
        var token = await tokenService
            .CreateAsync(account)
            .ConfigureAwait(false);

        var session = new Session(
            account,
            app,
            token);

        await sessionRepository
            .CreateAsync(session)
            .ConfigureAwait(false);

        return session;
    }

    public async Task DeleteAsync(AccountLogged accountLogged)
    {
        await sessionRepository
            .DeleteByTokenAsync(accountLogged.Token)
            .ConfigureAwait(false);
    }
}
