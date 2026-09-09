using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.UnitOfWork.Abstractions.Repositories;
using CQ.UnitOfWork.EfCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.AuthProvider.DataAccess.EfCore.Accounts;

internal sealed class AccountRepository(
AuthDbContext _context,
[FromKeyedServices(MapperKeyedService.DataAccess)] IMapper mapper)
: AuthDbContextRepository<AccountEfCore>(_context), IAccountRepository
{
    public async Task CreateAsync(Account account)
    {
        var accountEfCore = AccountEfCore.New(account);

        var accountRolesEfCore = account
            .Roles
            .ConvertAll(r => new AccountRole
            {
                AccountId = account.Id,
                RoleId = r.Id
            });

        var accountAppsEfCore = account
            .Apps
            .ConvertAll(a => new AccountApp
            {
                AccountId = account.Id,
                AppId = a.Id
            });

        await Entities
            .AddAsync(accountEfCore)
            .ConfigureAwait(false);

        await BaseContext
            .AddRangeAsync(accountRolesEfCore)
            .ConfigureAwait(false);

        await BaseContext
            .AddRangeAsync(accountAppsEfCore)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistByEmailAsync(string email)
    {
        return await ExistAsync(a => a.Email == email)
            .ConfigureAwait(false);
    }

    public async Task<Account> GetByEmailAsync(string email)
    {
        var account = await GetAsync(a => a.Email == email)
            .ConfigureAwait(false);

        return mapper.Map<Account>(account);
    }

    public async Task<Account> GetByIdAsync(
        Guid id,
        Guid appId)
    {
        var parentAppId = await _context
            .Apps
            .Where(a => a.Id == appId)
            .Select(a => a.FatherAppId)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        // From the Auth Provider Web API console every role is returned; any other
        // app only sees the roles scoped to it (or to its parent app).
        var isAuthWebApi = appId == AuthConstants.AUTH_WEB_API_APP_ID;

        var query =
            Entities
            .Include(a => a.Roles.Where(r =>
                isAuthWebApi || r.AppId == appId || r.AppId == parentAppId))
                .ThenInclude(r => r.Permissions)
            .Include(a => a.Tenant)
            .Include(a => a.Apps)
            .Where(a => a.Id == id)
            .AsNoTracking()
            .AsSplitQuery();

        var account = await query
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        AssertNullEntity(account, id, nameof(Account.Id));

        return mapper.Map<Account>(account);
    }

    public async Task<Account> GetByIdAsync(
        Guid id,
        params string[] includes)
    {
        var queryInclude = InsertIncludes(
            Entities,
            [.. includes]);

        var query = queryInclude
            .Where(a => a.Id == id)
            .AsNoTracking()
            .AsSplitQuery();

        var account = await query
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        AssertNullEntity(account, id.ToString(), nameof(Account.Id));

        return mapper.Map<Account>(account);
    }

    public async Task UpdateTenantByIdAsync(
        Guid id,
        Tenant tenant)
    {
        var queryGetAccount = Entities
            .Where(a => a.Id == id);

        var account = await queryGetAccount
            .FirstAsync()
            .ConfigureAwait(false);

        account.TenantId = tenant.Id;
    }

    public async Task AddRoleByIdAsync(
        Guid id,
        Guid roleId)
    {
        var accountRole = new AccountRole
        {
            AccountId = id,
            RoleId = roleId
        };

        await ConcreteContext
            .AccountsRoles
            .AddAsync(accountRole);
    }

    public Task RemoveRoleByIdAsync(
        Guid id,
        Guid roleId)
    {
        var query = ConcreteContext
            .AccountsRoles
            .Where(a => a.AccountId == id)
            .Where(a => a.RoleId == roleId);

        BaseContext.RemoveRange(query);

        return Task.CompletedTask;
    }

    public async Task<Pagination<Account>> GetAllAsync(
        Guid tenantId,
        Guid? appId,
        int page,
        int pageSize)
    {
        var query = Entities
            .Include(a => a.Roles)
            .Where(a => a.TenantId == tenantId)
            .Where(a => appId == null || a.Apps.Any(app => app.Id == appId))
            .AsSplitQuery();

        var paginated = await query
            .ToPaginateAsync(page, pageSize)
            .ConfigureAwait(false);

        return mapper.Map<Pagination<Account>>(paginated);
    }

    public async Task AddAppAsync(App app, AccountLogged accountLogged)
    {
        var accountApp = new AccountApp
        {
            AccountId = accountLogged.Id,
            AppId = app.Id
        };

        await BaseContext
            .AddAsync(accountApp)
            .ConfigureAwait(false);
    }

    /*
     Used in:
        - To update roles of account
     */
    public async Task<Account> GetByIdAsync(
        Guid id,
        AccountLogged accountLogged)
    {
        var query = Entities
            .Include(a => a.Apps)
            .Where(a => a.Id == id)
            .Where(a => a.TenantId == accountLogged.Tenant.Id)
            .AsNoTracking()
            .AsSplitQuery();

        var account = await query
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        AssertNullEntity(account, id, nameof(Account.Id));

        return mapper.Map<Account>(account);
    }

    public async Task DeleteRolesByIdAsync(
        List<Guid> rolesIds,
        AccountLogged accountLogged)
    {
        var query = ConcreteContext
            .AccountsRoles
            .Where(ar => rolesIds.Any(r => r == ar.RoleId) && ar.AccountId == accountLogged.Id);

        await query
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    public async Task AddRolesByIdAsync(
        List<Guid> rolesIds,
        AccountLogged accountLogged)
    {
        var roles = rolesIds
            .Select(ri => new AccountRole
            {
                RoleId = ri,
                AccountId = accountLogged.Id
            })
            .ToList();

        await BaseContext
            .AddRangeAsync(roles)
            .ConfigureAwait(false);
    }

    public async Task DeleteAndSaveByIdAsync(Guid id)
    {
        await DeleteAndSaveAsync(a => a.Id == id)
            .ConfigureAwait(false);
    }

    public async Task UpdateEmailVerifiedByIdAsync(Guid id)
    {
        var account = await base.GetByIdAsync(id).ConfigureAwait(false);

        account.IsEmailVerified = true;

        await UpdateAndSaveAsync(account).ConfigureAwait(false);
    }
}
