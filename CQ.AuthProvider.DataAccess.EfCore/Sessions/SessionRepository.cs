using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Sessions;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.AuthProvider.DataAccess.EfCore.Accounts;
using CQ.AuthProvider.DataAccess.EfCore.Roles;
using CQ.UnitOfWork.EfCore.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.AuthProvider.DataAccess.EfCore.Sessions;

internal sealed class SessionRepository(
    AuthDbContext context,
    [FromKeyedServices(MapperKeyedService.DataAccess)] IMapper mapper)
    : EfCoreRepository<SessionEfCore>(context),
    ISessionRepository
{
    async Task ISessionRepository.CreateAsync(Session session)
    {
        var sessionEfCore = new SessionEfCore(session);

        await CreateAsync(sessionEfCore).ConfigureAwait(false);
    }

    public async Task<Session> GetByTokenAsync(string token)
    {
        var session = await Entities
            .AsNoTracking()
            .Where(s => s.Token == token)
            .Select(s => new SessionEfCore
            {
                Id = s.Id,
                Token = s.Token,
                AppId = s.AppId,
                App = s.App,
                Account = new AccountEfCore
                {
                    Id = s.Account.Id,
                    FirstName = s.Account.FirstName,
                    LastName = s.Account.LastName,
                    FullName = s.Account.FullName,
                    Email = s.Account.Email,
                    TenantId = s.Account.TenantId,
                    Tenant = s.Account.Tenant,
                    Apps = s.Account.Apps.ToList(),
                    Roles = s.Account.Roles
                        .Where(r => r.AppId == s.AppId || r.AppId == s.App.FatherAppId)
                        .Select(r => new RoleEfCore
                        {
                            Id = r.Id,
                            Name = r.Name,
                            AppId = r.AppId,
                            Permissions = r.Permissions
                        })
                        .ToList()
                }
            })
            .AsSplitQuery()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        AssertNullEntity(session, token, nameof(Session.Token));

        return mapper.Map<Session>(session);
    }

    public async Task DeleteByTokenAsync(string token)
    {
        await DeleteAndSaveAsync(s => s.Token == token)
            .ConfigureAwait(false);
    }

    public async Task DeleteAndSaveByAccountIdAndAppIdAsync(
        Guid accountId,
        Guid appId)
    {
        await Entities
            .Where(s => s.AccountId == accountId && s.AppId == appId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }
}
