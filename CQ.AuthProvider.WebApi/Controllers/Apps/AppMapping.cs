using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.Blobs;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.Utility;

namespace CQ.AuthProvider.WebApi.Controllers.Apps;

internal sealed class AppProfile
    : Profile
{
    public AppProfile()
    {
        #region Father app (basic info exposed on list & detail)
        CreateMap<App, FatherAppBasicInfoResponse>();
        #endregion

        #region Get all
        this.CreatePaginationMap<App, AppBasicInfoResponse>();
        #endregion

        #region Get by id
        CreateMap<App, AppDetailInfoResponse>()
            .ForMember(destination => destination.Logo,
            options => options.MapFrom<LogoMultimediaResolver>());

        CreateMap<AccountDataSource, AccountDataSourceResponse>();
        #endregion

        #region Create
        CreateMap<App, AppCreatedResponse>();
        #endregion
    }
}

internal sealed class LogoMultimediaResolver(IBlobService blobService)
    : IValueResolver<App, AppDetailInfoResponse, LogoResponse>
{
    public LogoResponse Resolve(
        App source,
        AppDetailInfoResponse destination,
        LogoResponse destMember,
        ResolutionContext context)
    {
        var color = blobService.GetByKey(source.Logo.ColorKey);
        var light = blobService.GetByKey(source.Logo.LightKey);
        var dark = blobService.GetByKey(source.Logo.DarkKey);

        return new LogoResponse
        {
            Color = color,
            Light = light,
            Dark = dark,
        };
    }
}
