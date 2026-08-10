using System.Linq.Expressions;
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

    /// <summary>
    /// Brings everything the new access token has to carry, since a refresh
    /// mints it from the database rather than from the expiring token.
    /// </summary>
    public Task<Session?> GetOrDefaultByRefreshTokenHashAsync(string refreshTokenHash)
    {
        return GetOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash);
    }

    /// <summary>
    /// The opaque token path: every request carrying a pre JWT token lands
    /// here, so it has to load the same account data the token would have
    /// carried on its own.
    /// </summary>
    public Task<Session?> GetOrDefaultByTokenAsync(string token)
    {
        return GetOrDefaultAsync(s => s.Token == token);
    }

    private async Task<Session?> GetOrDefaultAsync(Expression<Func<SessionEfCore, bool>> predicate)
    {
        var session = await Entities
            .AsNoTracking()
            .Where(predicate)
            .Select(s => new SessionEfCore
            {
                Id = s.Id,
                Token = s.Token,
                RefreshTokenHash = s.RefreshTokenHash,
                RefreshTokenExpiresAt = s.RefreshTokenExpiresAt,
                AppId = s.AppId,
                App = s.App,
                Account = new AccountEfCore
                {
                    Id = s.Account.Id,
                    FirstName = s.Account.FirstName,
                    LastName = s.Account.LastName,
                    FullName = s.Account.FullName,
                    Email = s.Account.Email,
                    ProfilePictureKey = s.Account.ProfilePictureKey,
                    Locale = s.Account.Locale,
                    TimeZone = s.Account.TimeZone,
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

        return session == null
            ? null
            : mapper.Map<Session>(session);
    }

    public async Task UpdateRefreshTokenAsync(
        Guid sessionId,
        string refreshTokenHash,
        DateTime refreshTokenExpiresAt)
    {
        await Entities
            .Where(s => s.Id == sessionId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.RefreshTokenHash, refreshTokenHash)
                .SetProperty(s => s.RefreshTokenExpiresAt, refreshTokenExpiresAt))
            .ConfigureAwait(false);
    }

    public async Task DeleteByIdAsync(Guid sessionId)
    {
        await DeleteAndSaveAsync(s => s.Id == sessionId)
            .ConfigureAwait(false);
    }
}
