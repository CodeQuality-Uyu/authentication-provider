using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Blobs;
using CQ.AuthProvider.BusinessLogic.Permissions;
using CQ.AuthProvider.BusinessLogic.Roles;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.AuthProvider.WebApi.Controllers.Sessions;

namespace CQ.AuthProvider.WebApi.Controllers.Accounts;

internal sealed class AccountProfile
    : Profile
{
    public AccountProfile()
    {
        this.CreatePaginationMap<Account, AccountBasicInfoResponse>();

        CreateMap<Role, AccountRoleResponse>();

        #region Detail
        CreateMap<Account, AccountDetailResponse>();
        CreateMap<Role, AccountRoleDetailResponse>();
        CreateMap<Permission, AccountPermissionResponse>();
        #endregion Detail

        #region Create
        // Usado por /accounts/credentials (registro, email ya verificado en pasos previos) y por
        // /accounts/credentials-with-tenant — los dos loguean de una, Token siempre viene.
        CreateMap<CreateAccountResult, SessionCreatedResponse>()
            .ForMember(
            dest => dest.ProfilePicture,
            opt => opt.MapFrom<ProfilePictureResolver>())
            .ForMember(
            dest => dest.Token,
            opt => opt.MapFrom(
                src => $"Bearer {src.Token}"))
            ;
        #endregion

        #region Create credentials for
        CreateMap<Account, CreateCredentialsForResponse>();
        #endregion
    }
}

internal sealed class ProfilePictureResolver(IBlobService blobService)
    : IValueResolver<CreateAccountResult, SessionCreatedResponse, BlobReadResponse?>
{
    public BlobReadResponse? Resolve(
        CreateAccountResult source,
        SessionCreatedResponse destination,
        BlobReadResponse? destMember,
        ResolutionContext context)
    {
        if (source.ProfilePictureKey == null)
        {
            return null;
        }

        var blob = blobService.GetByKey(source.ProfilePictureKey);

        return blob;
    }
}

