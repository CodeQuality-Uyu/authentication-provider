using AutoMapper;
using CQ.AuthProvider.DataAccess.EfCore.Accounts;
using CQ.AuthProvider.DataAccess.EfCore.Apps;
using CQ.AuthProvider.DataAccess.EfCore.EmailVerifications;
using CQ.AuthProvider.DataAccess.EfCore.Invitations;
using CQ.AuthProvider.DataAccess.EfCore.Permissions;
using CQ.AuthProvider.DataAccess.EfCore.ResetPasswords;
using CQ.AuthProvider.DataAccess.EfCore.Roles;
using CQ.AuthProvider.DataAccess.EfCore.Sessions;
using CQ.AuthProvider.DataAccess.EfCore.Subscriptions;
using CQ.AuthProvider.DataAccess.EfCore.Tenants;

namespace CQ.AuthProvider.Tests.Infrastructure;

/// <summary>
/// El mismo <see cref="IMapper"/> que arma <c>EfCoreRepositoriesConfig</c> para la capa de datos.
/// </summary>
/// <remarks>
/// Se duplica la lista de profiles en vez de llamar a <c>ConfigureDataAccess</c> porque ese método
/// arma el contenedor entero y exige IConfiguration con un motor de base elegido. Si se agrega un
/// profile allá, hay que agregarlo acá.
/// </remarks>
internal static class DataAccessMapper
{
    internal static IMapper Create()
    {
        var config = new MapperConfiguration(config =>
        {
            config.DisableConstructorMapping();

            config.AddProfile<PermissionProfile>();
            config.AddProfile<RoleProfile>();
            config.AddProfile<InvitationProfile>();
            config.AddProfile<AppProfile>();
            config.AddProfile<AccountProfile>();
            config.AddProfile<TenantProfile>();
            config.AddProfile<ResetPasswordProfile>();
            config.AddProfile<EmailVerificationProfile>();
            config.AddProfile<SessionMapping>();
            config.AddProfile<SubscriptionProfile>();
        });

        return config.CreateMapper();
    }
}
