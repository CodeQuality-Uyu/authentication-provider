using CQ.AuthProvider.BusinessLogic.Accounts.Exceptions;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.EmailVerifications;
using CQ.AuthProvider.BusinessLogic.GoogleAuth;
using CQ.AuthProvider.BusinessLogic.Identities;
using CQ.AuthProvider.BusinessLogic.Roles;
using CQ.AuthProvider.BusinessLogic.Sessions;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.UnitOfWork.Abstractions;
using CQ.UnitOfWork.Abstractions.Repositories;
using CQ.Utility;
using System.Data;

namespace CQ.AuthProvider.BusinessLogic.Accounts;

internal sealed class AccountService(
    IAccountRepository accountRepository,
    IIdentityRepository identityRepository,
    ISessionInternalService _sessionService,
    IRoleRepository roleRepository,
    IAppInternalService _appService,
    IAppRepository appRepository,
    ITenantRepository tenantRepository,
    IEmailVerificationInternalService _emailVerificationService,
    ISessionRepository sessionRepository,
    IGoogleIdentityRepository googleIdentityRepository,
    IUnitOfWork unitOfWork)
    : IAccountInternalService
{
    private  const string DefaultPassword = "!12345678";

    #region Create
    public async Task<CreateAccountResult> CreateIdentityAndSaveAsync(
        Account account,
        string password,
        bool passwordIsHash = false)
    {
        var identity = new Identity
        {
            Id = account.Id,
            Email = account.Email,
            Password = password
        };

        await identityRepository
            .CreateAndSaveAsync(identity, passwordIsHash)
            .ConfigureAwait(false);

        await accountRepository
            .CreateAsync(account)
            .ConfigureAwait(false);

        var session = await _sessionService
            .CreateAsync(
            account,
            account.Apps[0])
            .ConfigureAwait(false);

        var result = new CreateAccountResult(
            account.Id,
            account.Email,
            account.FullName,
            account.FirstName,
            account.LastName,
            account.ProfilePictureKey,
            account.Locale,
            account.TimeZone,
            account.Apps[0],
            session.Token,
            account.Roles.ConvertAll(r => r.Name),
            account.Roles.SelectMany(r => r.Permissions.ConvertAll(p => p.Key)).ToList(),
            account.Tenant,
            account.IsEmailVerified);

        return result;
    }

    public async Task<CreateAccountResult> CreateAndSaveAsync(CreateAccountArgs args)
    {
        await AssertExistenseOfEmailAsync(args.Email).ConfigureAwait(false);

        // Se lee primero el app porque es el que decide si hace falta verificación o no.
        var app = await _appService
            .GetByIdAsync(args.AppId)
            .ConfigureAwait(false);

        if (app.RequiresEmailVerification)
        {
            // El email ya se tuvo que verificar ANTES de este paso (registro en 3 pasos: 1. pedir
            // email y mandar código, 2. validar código, 3. acá, con el resto de los datos). Si no
            // hay una verificación vigente para este email+código/token, no se crea la cuenta.
            if (Guard.IsNullOrEmpty(args.VerificationToken) && !args.VerificationCode.HasValue)
            {
                throw new EmailVerificationRequiredException(args.Email, app.Id);
            }

            await _emailVerificationService
                .ConsumeVerifiedAsync(args.Email, args.VerificationToken, args.VerificationCode)
                .ConfigureAwait(false);
        }

        var role = await roleRepository
                .GetDefaultByTenantIdAsync(args.RoleId, app.Id, app.Tenant.Id)
                .ConfigureAwait(false);

        var account = Account.New(
            args.Email,
            args.FirstName,
            args.LastName,
            args.ProfilePictureKey,
            args.Locale,
            args.TimeZone,
            role,
            app)
            // Si el app exige verificación, el email ya se probó en los pasos 1-2 — igual que con
            // Google, la cuenta se crea ya verificada y se loguea de una. Si no la exige, nadie
            // probó nada: queda sin verificar, y el login del app tampoco se la va a pedir.
            with
            { IsEmailVerified = app.RequiresEmailVerification };

        var result = await CreateAccountAsync(
            account,
            args.Password,
            args.IsPasswordHashed)
            .ConfigureAwait(false);

        return result;
    }

    private async Task<CreateAccountResult> CreateAccountAsync(
        Account account,
        string password,
        bool passwordIsHash)
    {
        var result = await CreateIdentityAndSaveAsync(
            account,
            password,
            passwordIsHash)
            .ConfigureAwait(false);

        try
        {
            await unitOfWork
                .CommitChangesAsync()
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            await identityRepository
                .DeleteAndSaveByIdAsync(account.Id)
                .ConfigureAwait(false);

            throw;
        }

        return result;
    }

    private async Task AssertExistenseOfEmailAsync(string email)
    {
        var existAuth = await accountRepository
            .ExistByEmailAsync(email)
            .ConfigureAwait(false);
        if (existAuth)
        {
            throw new InvalidOperationException($"Email ({email}) is in use");
        }
    }

    public async Task<Account> CreateAndSaveAsync(
        CreateAccountForArgs args,
        AccountLogged accountLogged)
    {
        await AssertExistenseOfEmailAsync(args.Email).ConfigureAwait(false);

        List<Guid> appIds = [accountLogged.AppLogged.Id];
        if (Guard.IsNotNull(args.AppIds) && args.AppIds.Count > 0)
        {
            appIds = args.AppIds;
        }

        var apps = await _appService
            .GetByIdAsync(appIds)
            .ConfigureAwait(false);

        var roles = await roleRepository
            .GetByIdAsync(args.RoleIds, appIds, accountLogged.Tenant.Id)
            .ConfigureAwait(false);

        var account = Account.New(
            args.Email,
            args.FirstName,
            args.LastName,
            args.ProfilePictureKey,
            args.Locale,
            args.TimeZone,
            roles,
            apps,
            accountLogged.Tenant)
            with
            { IsEmailVerified = true };

        var identity = Identity.NewForAccount(account, DefaultPassword);

        await identityRepository
            .CreateAndSaveAsync(identity)
            .ConfigureAwait(false);

        await accountRepository
            .CreateAsync(account)
            .ConfigureAwait(false);

        try
        {
            await unitOfWork
                .CommitChangesAsync()
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            await identityRepository
                .DeleteAndSaveByIdAsync(account.Id)
                .ConfigureAwait(false);

            throw;
        }

        await TransferTenantAndRemoveSeedAccountAsync(
            account,
            accountLogged)
            .ConfigureAwait(false);

        return account;
    }

    private async Task TransferTenantAndRemoveSeedAccountAsync(
        Account newAccount,
        AccountLogged accountLogged)
    {
        if (accountLogged.Id != AuthConstants.SEED_ACCOUNT_ID)
        {
            return;
        }

        if (!newAccount.HasPermission(AuthConstants.TENANT_OWNER_ROLE_ID))
        {
            await accountRepository
                .AddRoleByIdAsync(newAccount.Id, AuthConstants.TENANT_OWNER_ROLE_ID)
                .ConfigureAwait(false);
        }

        await roleRepository
            .DeleteAndSaveByIdAsync(AuthConstants.SEED_ROLE_ID)
            .ConfigureAwait(false);

        await identityRepository
            .DeleteAndSaveByIdAsync(AuthConstants.SEED_ACCOUNT_ID)
            .ConfigureAwait(false);

        await accountRepository
            .DeleteAndSaveByIdAsync(AuthConstants.SEED_ACCOUNT_ID)
            .ConfigureAwait(false);
    }

    public async Task<CreateAccountResult> CreateAndSaveWithTenantAsync(CreateAccountWithTenantArgs args)
    {
        await AssertExistenseOfEmailAsync(args.Email).ConfigureAwait(false);

        var tenant = new Tenant
        {
            Name = args.TenantName
        };

        await tenantRepository
        .CreateAsync(tenant)
        .ConfigureAwait(false);

        var account = Account.NewWithTenant(
            args.Email,
            args.FirstName,
            args.LastName,
            args.ProfilePictureKey,
            args.Locale,
            args.TimeZone,
            tenant);

        try
        {
            var result = await CreateIdentityAndSaveAsync(
                account,
                args.Password)
                .ConfigureAwait(false);

            await unitOfWork
                .CommitChangesAsync()
                .ConfigureAwait(false);

            return result;
        }
        catch (Exception)
        {
            await identityRepository
                .DeleteAndSaveByIdAsync(account.Id)
                .ConfigureAwait(false);

            throw;
        }
    }
    #endregion

    public async Task UpdatePasswordAsync(
        UpdatePasswordArgs args,
        AccountLogged accountLogged)
    {
        await identityRepository
            .UpdatePasswordByIdAsync(
            accountLogged.Id,
            args.OldPassword,
            args.NewPassword)
            .ConfigureAwait(false);
    }

    public async Task AssertByEmailAsync(string email)
    {
        var existAccount = await accountRepository
            .ExistByEmailAsync(email)
            .ConfigureAwait(false);

        if (existAccount)
        {
            throw new InvalidOperationException($"Email {email} is used");
        }
    }

    /// <remarks>
    /// Con la vista global (<c>getallcrosstenant-account</c>) no se filtra por tenant salvo que se
    /// pida uno, y <paramref name="appId"/> puede ser de cualquier tenant. Sin ella, siempre el
    /// tenant de la sesión, y pedir otro da 403 (ver <see cref="AccountLogged.ResolveTenantFilter"/>).
    /// </remarks>
    public async Task<Pagination<Account>> GetAllAsync(
        Guid? tenantId,
        Guid? appId,
        int page,
        int pageSize,
        AccountLogged accountLogged)
    {
        var tenantFilter = accountLogged.ResolveTenantFilter(tenantId);

        var accounts = await accountRepository
            .GetAllAsync(tenantFilter, appId, page, pageSize)
            .ConfigureAwait(false);

        return accounts;
    }

    /// <remarks>
    /// Una cuenta de otro tenant da 404, igual que un id inexistente, salvo con la vista global.
    /// Antes no se validaba el tenant: con <c>getall-account</c> y el id se leía cualquier cuenta.
    /// </remarks>
    public async Task<Account> GetByIdAsync(
        Guid id,
        AccountLogged accountLogged)
    {
        var tenantId = accountLogged.HasCrossTenantView()
            ? (Guid?)null
            : accountLogged.Tenant.Id;

        var account = await accountRepository
            .GetByIdAsync(id, accountLogged.AppLogged.Id, tenantId)
            .ConfigureAwait(false);

        return account;
    }

    /// <summary>
    /// Deja los roles de la cuenta <paramref name="id"/> en el conjunto indicado en
    /// <paramref name="args"/>.
    /// </summary>
    /// <remarks>
    /// Antes este método ignoraba por completo el <paramref name="id"/> y operaba sobre
    /// <c>accountLogged</c>: calculaba el diff contra los roles de la cuenta logueada y se los
    /// modificaba a ella. O sea que no solo no hacía lo que dice el endpoint, sino que cualquiera
    /// con el permiso <c>updateroles-account</c> podía asignarse a sí mismo cualquier rol del
    /// tenant. Ahora opera sobre la cuenta pedida, y exige que sea del mismo tenant.
    /// <para>
    /// Las bajas se limitan a los roles que el llamador <b>puede ver</b> (su alcance efectivo). Sin
    /// eso, mandar la lista deseada le borraría a la cuenta los roles de apps que el llamador no ve
    /// — por ejemplo un admin de una app hija dejando sin roles a un usuario en la app del padre.
    /// </para>
    /// </remarks>
    public async Task UpdateRolesAsync(
        Guid id,
        UpdateRolesArgs args,
        AccountLogged accountLogged)
    {
        var target = await accountRepository
            .GetRolesSnapshotByIdAsync(id)
            .ConfigureAwait(false);

        if (!target.HasValue)
        {
            throw new InvalidOperationException($"The account ({id}) does not exist");
        }

        if (target.Value.TenantId != accountLogged.Tenant.Id)
        {
            throw new InvalidOperationException($"The account ({id}) does not belong to the tenant");
        }

        var currentRoleIds = target.Value.RoleIds;

        var visibleCurrentRoles = await roleRepository
            .GetAllByIdsAsync(currentRoleIds, accountLogged)
            .ConfigureAwait(false);

        var rolesToDelete = visibleCurrentRoles
            .ConvertAll(r => r.Id)
            .Where(r => !args.RoleIds.Contains(r))
            .ToList();
        if (rolesToDelete.Count != 0)
        {
            await accountRepository
                .DeleteRolesByIdAsync(id, rolesToDelete)
                .ConfigureAwait(false);
        }

        var newRoles = args
            .RoleIds
            .Where(ri => !currentRoleIds.Contains(ri))
            .ToList();
        if (newRoles.Count != 0)
        {
            var roles = await roleRepository
                .GetAllByIdsAsync(newRoles, accountLogged)
                .ConfigureAwait(false);

            // GetAllByIdsAsync ya filtra por tenant y por alcance efectivo de las apps de la
            // cuenta logueada, así que un rol que no vuelve es un rol que el llamador no puede
            // asignar. No hace falta un segundo chequeo contra r.AppId — con el alcance multi-app
            // un rol heredado tiene un AppId que no está entre las apps de la cuenta y sería
            // rechazado sin motivo.
            if (roles.Count != newRoles.Count)
            {
                throw new InvalidOperationException("Some roles don't belong to tenant");
            }

            await accountRepository
                .AddRolesByIdAsync(id, newRoles)
                .ConfigureAwait(false);
        }

        if (rolesToDelete.Count != 0 || newRoles.Count != 0)
        {
            await unitOfWork
                .CommitChangesAsync()
                .ConfigureAwait(false);
        }
    }

    public Task DeleteFromAppAsync(AccountLogged accountLogged)
    {
        return RemoveFromAppAsync(
            accountLogged,
            accountLogged.AppLogged.Id);
    }

    /// <remarks>
    /// Mismo alcance que <see cref="UpdateRolesAsync"/>: solo cuentas del tenant de la sesión,
    /// aunque se tenga la vista global, que es de lectura. Una cuenta de otro tenant da 404.
    /// </remarks>
    public async Task DeleteFromAppByIdAsync(
        Guid id,
        Guid? appId,
        AccountLogged accountLogged)
    {
        if (id == accountLogged.Id)
        {
            throw new AccountSelfDeletionException(id);
        }

        var targetAppId = appId ?? accountLogged.AppLogged.Id;

        await AssertCanReachAppAsync(
            targetAppId,
            accountLogged,
            AuthConstants.DELETE_ACCOUNT_OF_CHILD_APP_PERMISSION_KEY,
            AuthConstants.DELETE_ACCOUNT_OF_CROSS_APP_PERMISSION_KEY)
            .ConfigureAwait(false);

        var account = await accountRepository
            .GetByIdAsync(id, targetAppId, accountLogged.Tenant.Id)
            .ConfigureAwait(false);

        await RemoveFromAppAsync(account, targetAppId).ConfigureAwait(false);
    }

    /// <summary>
    /// Resuelve contra la base lo que <see cref="AccountLogged.AssertCanReachApp"/> necesita: si
    /// la app es del tenant y si desciende de la logueada.
    /// </summary>
    private async Task AssertCanReachAppAsync(
        Guid appId,
        AccountLogged accountLogged,
        string childAppPermissionKey,
        string crossAppPermissionKey)
    {
        var appLoggedId = accountLogged.AppLogged.Id;
        if (appId == appLoggedId)
        {
            return;
        }

        // Candado duro del tenant, aunque la regla ya no pueda salir de él: una app de otro tenant
        // se rechaza igual que una sin alcance, sin revelar si existe.
        var inTenant = await appRepository
            .GetExistingIdsInTenantAsync([appId], accountLogged.Tenant.Id)
            .ConfigureAwait(false);
        if (inTenant.Count == 0)
        {
            throw new CrossAppAccessDeniedException(appId, "the app to belong to the tenant");
        }

        var descendants = await appRepository
            .GetDescendantIdsAsync([appId], appLoggedId)
            .ConfigureAwait(false);

        accountLogged.AssertCanReachApp(
            appId,
            descendants.Contains(appId),
            childAppPermissionKey,
            crossAppPermissionKey);
    }

    /// <summary>
    /// Saca a <paramref name="account"/> del app <paramref name="appId"/> y cierra sus sesiones
    /// ahí; si no le queda otra app, borra la cuenta entera (credenciales incluidas). Qué caso
    /// aplica lo decide <see cref="Account.ResolveRemovalFrom"/>.
    /// </summary>
    private async Task RemoveFromAppAsync(
        Account account,
        Guid appId)
    {
        var removal = account.ResolveRemovalFrom(appId);

        // Sin la fila AccountApp el login al app se rechaza, y sin sesiones el token deja de
        // validar. Las sesiones van al final para que, si algo falla antes, quien llama pueda
        // reintentar con el mismo token (en DELETE /me es justo el de la cuenta que se saca).
        if (removal == AppRemoval.RemoveApp)
        {
            await accountRepository
                .RemoveAppAndSaveByIdAsync(account.Id, appId)
                .ConfigureAwait(false);

            await sessionRepository
                .DeleteAndSaveByAccountIdAndAppIdAsync(account.Id, appId)
                .ConfigureAwait(false);

            return;
        }

        // Las identidades viven en otra base, asi que no hay transaccion comun; van primero porque
        // son idempotentes y, si falla el borrado de la cuenta, la sesion sigue viva para
        // reintentar. Borrar la cuenta cascadea sesiones, roles, apps y reseteos de contrasena.
        await identityRepository
            .DeleteAndSaveByIdAsync(account.Id)
            .ConfigureAwait(false);

        await googleIdentityRepository
            .DeleteAndSaveByAccountIdAsync(account.Id)
            .ConfigureAwait(false);

        await accountRepository
            .DeleteAndSaveByIdAsync(account.Id)
            .ConfigureAwait(false);
    }
}
