using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.UnitOfWork.Abstractions.Repositories;
using CQ.UnitOfWork.EfCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.AuthProvider.DataAccess.EfCore.Apps;

internal sealed class AppRepository(
    AuthDbContext context,
    [FromKeyedServices(MapperKeyedService.DataAccess)] IMapper _mapper)
    : AuthDbContextRepository<AppEfCore>(context),
    IAppRepository
{
    public async Task CreateAsync(App app)
    {
        var appEfCore = _mapper.Map<AppEfCore>(app);

        await CreateAsync(appEfCore).ConfigureAwait(false);

        await AddAncestorsOfNewAppAsync(appEfCore).ConfigureAwait(false);
    }

    /// <summary>
    /// Arma el cierre de ancestros de una app recién creada: su padre en depth 1, más los
    /// ancestros del padre corridos un nivel.
    /// </summary>
    /// <remarks>
    /// La app nueva todavía no tiene hijos, así que no hay nada más del árbol que recalcular.
    /// Las filas quedan en el change tracker a propósito, para que se commiteen junto con la app
    /// en el mismo <c>SaveChanges</c> del unit of work: si el alta falla, el cierre no queda
    /// apuntando a una app que no existe.
    /// </remarks>
    private async Task AddAncestorsOfNewAppAsync(AppEfCore app)
    {
        if (!app.FatherAppId.HasValue)
        {
            return;
        }

        var fatherId = app.FatherAppId.Value;

        var fatherAncestors = await ConcreteContext
            .AppsAncestors
            .Where(aa => aa.AppId == fatherId)
            .Select(aa => new { aa.AncestorId, aa.Depth })
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);

        var rows = new List<AppAncestor>
        {
            new()
            {
                AppId = app.Id,
                AncestorId = fatherId,
                Depth = 1,
            },
        };

        rows.AddRange(fatherAncestors.ConvertAll(fa => new AppAncestor
        {
            AppId = app.Id,
            AncestorId = fa.AncestorId,
            Depth = fa.Depth + 1,
        }));

        await ConcreteContext
            .AppsAncestors
            .AddRangeAsync(rows)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameInTenantAsync(
        string name,
        Guid tenantId)
    {
        var query = ConcreteContext
            .Apps
            .Where(a => EF.Functions.Like(a.Name, name))
            .Where(a => a.TenantId == tenantId);

        var exist = await query
            .AnyAsync()
            .ConfigureAwait(false);

        return exist;
    }

    public async Task<List<App>> GetByIdAsync(List<Guid> ids)
    {
        var apps = await GetAllAsync(a => ids.Contains(a.Id))
            .ConfigureAwait(false);

        return _mapper.Map<List<App>>(apps);
    }

    async Task<App> IAppRepository.GetByIdAsync(Guid id)
    {
        var query = Entities
            .Include(a => a.Tenant)
            .Include(a => a.FatherApp)
            .Where(a => a.Id == id);

        var app = await query
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        AssertNullEntity(app, id, nameof(App.Id));

        return _mapper.Map<App>(app);
    }

    public async Task<Pagination<App>> GetPaginationAsync(
        Guid tenantId,
        Guid? fatherAppId,
        int page,
        int pageSize)
    {
        var query = Entities
            .Include(a => a.FatherApp)
            .Where(a => a.TenantId == tenantId)
            .Where(a => fatherAppId == null || fatherAppId == Guid.Empty || a.FatherAppId == fatherAppId);

        var pagination = await query
            .ToPaginateAsync(page, pageSize)
            .ConfigureAwait(false);

        return _mapper.Map<Pagination<App>>(pagination);
    }

    public async Task<App> GetOrDefaultByDefaultAsync(Guid tenantId)
    {
        var query = Entities
            .Where(a => a.IsDefault)
            .Where(a => a.TenantId == tenantId);

        var app = await query
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        return _mapper.Map<App>(app);
    }

    public async Task RemoveDefaultByIdAsync(Guid id)
    {
        var app = await GetByIdAsync(id).ConfigureAwait(false);

        app.IsDefault = false;
    }

    public async Task UpdateAndSaveByIdAsync(
        Guid id,
        string name,
        AccountDataSource? accountDataSource,
        string? googleClientId,
        bool requiresEmailVerification)
    {
        await Entities
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(a => a.Name, name)
                .SetProperty(a => a.AccountDataSource, accountDataSource)
                .SetProperty(a => a.GoogleClientId, googleClientId)
                .SetProperty(a => a.RequiresEmailVerification, requiresEmailVerification))
            .ConfigureAwait(false)
            ;
    }

    public async Task UpdateAndSaveFatherByIdAsync(
        Guid id,
        Guid? fatherAppId,
        Guid tenantId)
    {
        await Entities
            .Where(a => a.Id == id)
            .Where(a => a.TenantId == tenantId)
            .ExecuteUpdateAsync(setter => setter.SetProperty(a => a.FatherAppId, fatherAppId))
            .ConfigureAwait(false);

        await RebuildAncestorsOfTenantAsync(tenantId).ConfigureAwait(false);
    }

    /// <summary>
    /// Recalcula el cierre de ancestros de todas las apps del tenant a partir de
    /// <see cref="AppEfCore.FatherAppId"/>, que es la fuente de verdad del árbol.
    /// </summary>
    /// <remarks>
    /// Recalcula el tenant entero y no solo el subárbol movido. Es más trabajo del necesario, pero
    /// re-parentar es una acción de administración muy poco frecuente y las apps por tenant son
    /// pocas, así que se paga eso a cambio de que no exista la clase de bug del recálculo parcial:
    /// filas viejas que quedan colgadas y aparecen después como permisos que sobran o que faltan,
    /// sin error de por medio.
    /// <para>
    /// El diff se hace contra las filas existentes en vez de borrar todo y volver a insertar, por
    /// dos razones: no hay una ventana en la que el cierre del tenant quede vacío (y con él los
    /// permisos heredados de todos), y EF no tiene que ordenar un DELETE y un INSERT sobre la
    /// misma clave primaria dentro del mismo <c>SaveChanges</c>.
    /// </para>
    /// </remarks>
    private async Task RebuildAncestorsOfTenantAsync(Guid tenantId)
    {
        var fatherOf = await Entities
            .Where(a => a.TenantId == tenantId)
            .Select(a => new { a.Id, a.FatherAppId })
            .AsNoTracking()
            .ToDictionaryAsync(a => a.Id, a => a.FatherAppId)
            .ConfigureAwait(false);

        var appIds = fatherOf.Keys.ToList();

        var desired = new List<AppAncestor>();

        foreach (var appId in appIds)
        {
            var visited = new HashSet<Guid>();
            var current = fatherOf[appId];
            var depth = 1;

            // El corte por ciclo no es defensivo de más: GetAncestorIdsAsync ya se defendía de
            // datos con ciclos, y acá un ciclo sería un loop infinito.
            while (current.HasValue && current.Value != appId && visited.Add(current.Value))
            {
                desired.Add(new AppAncestor
                {
                    AppId = appId,
                    AncestorId = current.Value,
                    Depth = depth,
                });

                // Un padre que no está en el diccionario es un padre de otro tenant, que el árbol
                // no permite. Ahí la cadena se corta.
                current = fatherOf.TryGetValue(current.Value, out var father)
                    ? father
                    : null;

                depth++;
            }
        }

        var existing = await ConcreteContext
            .AppsAncestors
            .Where(aa => appIds.Contains(aa.AppId))
            .ToListAsync()
            .ConfigureAwait(false);

        var desiredByKey = desired.ToDictionary(d => (d.AppId, d.AncestorId));

        var obsolete = existing
            .Where(e => !desiredByKey.ContainsKey((e.AppId, e.AncestorId)))
            .ToList();

        ConcreteContext
            .AppsAncestors
            .RemoveRange(obsolete);

        var existingKeys = existing
            .Select(e => (e.AppId, e.AncestorId))
            .ToHashSet();

        var missing = desired
            .Where(d => !existingKeys.Contains((d.AppId, d.AncestorId)))
            .ToList();

        await ConcreteContext
            .AppsAncestors
            .AddRangeAsync(missing)
            .ConfigureAwait(false);

        // Los pares que sobreviven pueden haber cambiado de profundidad si el movimiento fue
        // dentro de la misma rama.
        existing
            .Where(e => desiredByKey.ContainsKey((e.AppId, e.AncestorId)))
            .ToList()
            .ForEach(e =>
            {
                var entry = ConcreteContext.Entry(e);

                entry.Property(x => x.Depth).CurrentValue =
                    desiredByKey[(e.AppId, e.AncestorId)].Depth;
            });

        await ConcreteContext
            .SaveChangesAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Guid>> GetDescendantIdsAsync(
        List<Guid> appIds,
        Guid ancestorId)
    {
        if (appIds.Count == 0)
        {
            return [];
        }

        // Se resuelve contra la tabla de cierre y no subiendo FatherAppId: es la misma informacion
        // y aca alcanza una sola query. Tener presente que el cierre es un indice derivado — si
        // quedara stale, esto podria rechazar un grant valido o aceptar uno que dejo de serlo.
        return await ConcreteContext
            .AppsAncestors
            .Where(aa => appIds.Contains(aa.AppId))
            .Where(aa => aa.AncestorId == ancestorId)
            .Select(aa => aa.AppId)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Guid>> GetExistingIdsInTenantAsync(
        List<Guid> appIds,
        Guid tenantId)
    {
        if (appIds.Count == 0)
        {
            return [];
        }

        return await Entities
            .Where(a => a.TenantId == tenantId && appIds.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Guid>> GetAncestorIdsAsync(
        Guid appId,
        Guid tenantId)
    {
        var ancestors = new List<Guid>();
        Guid? current = appId;

        while (current.HasValue)
        {
            var father = await Entities
                .Where(a => a.Id == current.Value && a.TenantId == tenantId)
                .Select(a => a.FatherAppId)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            // Stop at the root, or if existing data already contains a cycle.
            if (!father.HasValue || father.Value == appId || ancestors.Contains(father.Value))
            {
                break;
            }

            ancestors.Add(father.Value);
            current = father.Value;
        }

        return ancestors;
    }

    public async Task<List<App>> GetByEmailAccountAsync(string email)
    {
        var query = ConcreteContext
            .AccountsApps
            .Include(a => a.App.Tenant)
            .Where(a => a.Account.Email == email)
            .Select(a => a.App);

        var apps = await query
            .ToListAsync()
            .ConfigureAwait(false);

        return _mapper.Map<List<App>>(apps);
    }
}
