using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.EmailVerifications;

namespace CQ.AuthProvider.DataAccess.EfCore.EmailVerifications;
internal sealed class EmailVerificationProfile
    : Profile
{
    public EmailVerificationProfile()
    {
        CreateMap<EmailVerificationEfCore, EmailVerification>()
            .ForMember(
            destination => destination.Account,
            options => options.MapFrom(
                source => new Account
                {
                    Id = source.AccountId
                }));
    }
}
