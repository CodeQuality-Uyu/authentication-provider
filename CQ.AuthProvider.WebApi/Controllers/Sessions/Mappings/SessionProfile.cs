using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Abstractions.Sessions;
using CQ.AuthProvider.WebApi.Controllers.Sessions.Models;

namespace CQ.AuthProvider.WebApi.Controllers.Sessions.Mappings;

internal sealed class SessionProfile
    : Profile
{
    public SessionProfile()
    {
        CreateMap<Session, SessionCreatedResponse>()
            .ConvertUsing((source, destination, options) => new SessionCreatedResponse
            {
                AccountId = source.Account.Id,
                Email = source.Account.Email,
                FirstName = source.Account.FirstName,
                LastName = source.Account.LastName,
                FullName = source.Account.FullName,
                ProfilePictureUrl = source.Account.ProfilePictureId,
                Token = source.Token,
                Roles = source.Account.Roles.ConvertAll(r => r.Name),
                Permissions = source
                .Account
                .Roles
                .SelectMany(r => r.Permissions)
                .Select(p => p.Key)
            .ToList()
            });
    }
}
