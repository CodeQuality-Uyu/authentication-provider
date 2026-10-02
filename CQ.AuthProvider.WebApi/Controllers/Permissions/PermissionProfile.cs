using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Permissions;
using CQ.AuthProvider.BusinessLogic.Utils;

namespace CQ.AuthProvider.WebApi.Controllers.Permissions;

internal sealed class PermissionProfile
    : Profile
{
    public PermissionProfile()
    {
        #region Get all
        CreateMap<App, PermissionAppBasicInfoResponse>();
        this.CreatePaginationMap<Permission, PermissionBasicInfoResponse>();
        #endregion

        #region Create
        CreateMap<Permission, PermissionCreatedResponse>();
        #endregion
    }
}
