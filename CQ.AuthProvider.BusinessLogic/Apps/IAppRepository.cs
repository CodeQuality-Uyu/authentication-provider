using CQ.UnitOfWork.Abstractions.Repositories;

namespace CQ.AuthProvider.BusinessLogic.Apps;

public interface IAppRepository
{
    Task<App> GetByIdAsync(Guid id);

    Task<List<App>> GetByIdAsync(List<Guid> ids);

    Task<App> GetOrDefaultByDefaultAsync(Guid tenantId);

    Task RemoveDefaultByIdAsync(Guid id);

    Task<bool> ExistsByNameInTenantAsync(
        string name,
        Guid tenantId);

    Task CreateAsync(App app);

    Task<Pagination<App>> GetPaginationAsync(
        Guid tenantId,
        Guid? fatherAppId,
        int page,
        int pageSize);

    Task UpdateAndSaveByIdAsync(
        Guid id,
        string name,
        AccountDataSource? accountDataSource,
        string? googleClientId,
        bool requiresEmailVerification);

    Task UpdateAndSaveLogoByIdAsync(
        Guid id,
        Logo logo);

    /// <summary>
    /// Las keys de logo que usan las apps del tenant, salvo <paramref name="excludingAppId"/>.
    /// </summary>
    /// <remarks>
    /// Una app cliente creada sin logo propio guarda las mismas keys que su padre: antes de
    /// borrar un logo reemplazado hay que saber si otra app lo sigue usando.
    /// </remarks>
    Task<HashSet<string>> GetLogoKeysInUseAsync(
        Guid tenantId,
        Guid excludingAppId);

    Task UpdateAndSaveFatherByIdAsync(
        Guid id,
        Guid? fatherAppId,
        Guid tenantId);

    /// <summary>Returns the subset of <paramref name="appIds"/> that exist in the tenant.</summary>
    Task<List<Guid>> GetExistingIdsInTenantAsync(
        List<Guid> appIds,
        Guid tenantId);

    /// <summary>Returns the ids of the ancestor apps of <paramref name="appId"/> (walking the father chain).</summary>
    Task<List<Guid>> GetAncestorIdsAsync(
        Guid appId,
        Guid tenantId);

    /// <summary>
    /// De <paramref name="appIds"/>, las que son descendientes de <paramref name="ancestorId"/>
    /// (a cualquier profundidad).
    /// </summary>
    Task<List<Guid>> GetDescendantIdsAsync(
        List<Guid> appIds,
        Guid ancestorId);

    Task<List<App>> GetByEmailAccountAsync(string email);
}
