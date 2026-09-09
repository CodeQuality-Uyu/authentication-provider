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

    Task UpdateAndSaveColorsByIdAsync(
        Guid id,
        Background updates);

    Task UpdateAndSaveByIdAsync(
        Guid id,
        string name,
        AccountDataSource? accountDataSource,
        string? googleClientId);

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

    Task<List<App>> GetByEmailAccountAsync(string email);
}
