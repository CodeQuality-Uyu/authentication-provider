using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Blobs;
using CQ.AuthProvider.BusinessLogic.Tokens;
using CQ.AuthProvider.WebApi.Controllers.Sessions;

namespace CQ.AuthProvider.WebApi.Controllers.Me;

internal sealed class MeProfile
    : Profile
{
    public MeProfile()
    {
        CreateMap<AccountLogged, SessionCreatedResponse>()
            .ForMember(
            dest => dest.ProfilePicture,
            opt => opt.MapFrom<ProfilePictureResolver>())
            .ForMember(
            dest => dest.Token,
            opt => opt.MapFrom(
                src => $"Bearer {src.Token}"))
            // Read off the token the request came in with: a subscription token
            // is a guid too, so it reports as opaque, which is how the client
            // has to treat it anyway.
            .ForMember(
            dest => dest.TokenFormat,
            opt => opt.MapFrom(
                src => TokenFormats.Of(src.Token)))
            // Both only exist when a session is issued, and /me reports on an
            // already issued one.
            .ForMember(
            dest => dest.ExpiresIn,
            opt => opt.Ignore())
            .ForMember(
            dest => dest.RefreshToken,
            opt => opt.Ignore())
            .ForMember(
            dest => dest.Roles,
            opt => opt.MapFrom(
                src => src.Roles.ConvertAll(r => r.Name)))
            .ForMember(
            dest => dest.Permissions,
            opt => opt.MapFrom(
                src => src.Roles.SelectMany(r => r.Permissions).ToList().ConvertAll(p => p.Key)))
            ;
    }
}

internal sealed class ProfilePictureResolver(IBlobService blobService)
    : IValueResolver<AccountLogged, SessionCreatedResponse, BlobReadResponse?>
{
    public BlobReadResponse? Resolve(
        AccountLogged source,
        SessionCreatedResponse destination,
        BlobReadResponse? destMember,
        ResolutionContext context)
    {
        if (source.ProfilePictureKey == null)
        {
            return null;
        }

        return blobService.GetByKey(source.ProfilePictureKey);
    }
}
