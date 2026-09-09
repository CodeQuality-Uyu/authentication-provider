using CQ.AuthProvider.BusinessLogic.GoogleAuth;
using CQ.AuthProvider.BusinessLogic.Identities;
using CQ.Extensions.ServiceCollection;
using CQ.IdentityProvider.EfCore.Identities;
using CQ.UnitOfWork.EfCore.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GoogleIdentityRepository = CQ.IdentityProvider.EfCore.GoogleAuth.GoogleIdentityRepository;

namespace CQ.IdentityProvider.EfCore.AppConfig;

public static class EfCoreRepositoriesConfig
{
    public static IServiceCollection ConfigureIdentityProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddScoped<PasswordHasher<string>>()
            .AddAbstractionRepository<Identity, IIdentityRepository, IdentityRepository>(LifeTime.Scoped)
            .AddScoped<IGoogleIdentityRepository, GoogleIdentityRepository>()
            ;

        return services;
    }
}
