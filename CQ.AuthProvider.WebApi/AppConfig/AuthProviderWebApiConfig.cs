using CQ.ApiElements.AppConfig;
using CQ.AuthProvider.BusinessLogic.Abstractions.AppConfig;
using CQ.AuthProvider.DataAccess.EfCore.AppConfig;
using CQ.AuthProvider.WebApi.Filters.Exception;
using CQ.Extensions.ServiceCollection;
using CQ.IdentityProvider.EfCore.AppConfig;

namespace CQ.AuthProvider.WebApi.AppConfig;

internal static class AuthProviderWebApiConfig
{
    public static IServiceCollection ConfigureApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddExceptionGlobalHandlerService<CQAuthExceptionRegistryService>(LifeTime.Singleton)
            
            .AddMappings()

            .ConfigureServices()

            .ConfigureDataAccess(configuration)

            .ConfigureIdentityProvider(configuration)

            .AddFakeAuthentication<FakeAccountLogged>(configuration)
            ;

        return services;
    }

    private static IServiceCollection AddMappings(
        this IServiceCollection services)
    {
        services
            //.AddAutoMapper(config =>
            //{
            //    config.AddProfile
            //})
            .AddAutoMapper(typeof(Program));

        return services;
    }
}
