using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.UnitOfWork.Abstractions.Repositories;

namespace CQ.AuthProvider.BusinessLogic.Apps;

public interface IAppService
{
    Task<App> CreateAsync(
        CreateAppArgs args,
        AccountLogged accountLogged);

    Task<App> CreateClientAsync(
        CreateClientAppArgs args,
        AccountLogged accountLogged);

    Task<Pagination<App>> GetPaginationAsync(
        Guid? tenantId,
        Guid? fatherAppId,
        int page,
        int pageSize,
        AccountLogged accountLogged);

    Task<App> GetByIdAsync(Guid id);

    Task UpdateFatherByIdAsync(
        Guid id,
        UpdateAppFatherArgs args,
        AccountLogged accountLogged);

    Task UpdateByIdAsync(
        Guid id,
        UpdateAppArgs args,
        AccountLogged accountLogged);

    Task UpdateLogoByIdAsync(
        Guid id,
        UpdateAppLogoArgs args,
        AccountLogged accountLogged);

    Task<List<App>> GetByEmailAccountAsync(string email);
}

internal interface IAppInternalService
    : IAppService
{
    Task<List<App>> GetByIdAsync(List<Guid> ids);
}
