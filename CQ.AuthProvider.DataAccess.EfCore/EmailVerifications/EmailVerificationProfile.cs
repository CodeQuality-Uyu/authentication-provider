using AutoMapper;
using CQ.AuthProvider.BusinessLogic.EmailVerifications;

namespace CQ.AuthProvider.DataAccess.EfCore.EmailVerifications;
internal sealed class EmailVerificationProfile
    : Profile
{
    public EmailVerificationProfile()
    {
        CreateMap<EmailVerificationEfCore, EmailVerification>();
    }
}
