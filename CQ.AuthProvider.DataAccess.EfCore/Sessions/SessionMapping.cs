using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Sessions;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.AuthProvider.BusinessLogic.Tokens;

namespace CQ.AuthProvider.DataAccess.EfCore.Sessions;

internal sealed class SessionMapping
    : Profile
{
    public SessionMapping()
    {
        CreateMap<SessionEfCore, Session>()
            .ConvertUsing((source, destination, options) => new Session
            {
                Id = source.Id,
                // A stored token means an opaque session; a stored refresh hash
                // means a JWT one. They are never both set.
                TokenFormat = source.Token == null
                    ? TokenFormat.Jwt
                    : TokenFormat.Opaque,
                Token = source.Token!,
                RefreshTokenHash = source.RefreshTokenHash,
                RefreshTokenExpiresAt = source.RefreshTokenExpiresAt,
                Account = options.Mapper.Map<Account>(source.Account),
                App = new App
                {
                    Id = source.AppId,
                    Name = source.App?.Name ?? string.Empty,
                    Logo = source.App?.Logo ?? default,
                    Tenant = new Tenant
                    {
                        Id = source.Account.TenantId,
                    }
                },
            });
    }
}
