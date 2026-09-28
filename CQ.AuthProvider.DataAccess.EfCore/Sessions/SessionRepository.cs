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
                    // Misma regla que EffectiveScope, que es la fuente de verdad. Va inline y no
                    // con esas extensiones porque esto es una proyeccion sobre una navegacion, no
                    // una query raiz. Si cambia la regla, cambia aca tambien.
                    //
                    // Se resuelve dentro de la misma query a proposito: esto corre en CADA request
                    // autenticado, y para eso existe la tabla de cierre AppsAncestors — sin ella
                    // habria que subir la cadena de FatherAppId con un query por nivel.
                    Roles = s.Account.Roles
                        .Where(r =>
                            r.AppId == s.AppId
                            || (r.IsPublic && context.AppsAncestors.Any(aa =>
                                aa.AppId == s.AppId &&
                                aa.AncestorId == r.AppId))
                            || context.RolesApps.Any(ra =>
                                ra.RoleId == r.Id &&
                                ra.AppId == s.AppId))
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
