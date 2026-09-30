using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.AuthProvider.BusinessLogic.Utils;

namespace CQ.AuthProvider.WebApi.Controllers.Tenants;

internal sealed class TenantMapping
    : Profile
{
    public TenantMapping()
    {
        #region Create session
        CreateMap<Tenant, TenantOfAccountBasicInfoResponse>();
        #endregion

        #region Get paginated
        this.CreatePaginationMap<Tenant, TenantBasicInfoResponse>();
        CreateMap<Account, OwnerTenantBasicInfoResponse>();
        #endregion
    }
}
