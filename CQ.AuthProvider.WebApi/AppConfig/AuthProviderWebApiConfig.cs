using CQ.ApiElements.AppConfig;
using CQ.AuthProvider.BusinessLogic.Abstractions.AppConfig;
using CQ.AuthProvider.DataAccess.EfCore;
using CQ.AuthProvider.DataAccess.EfCore.AppConfig;
using CQ.AuthProvider.WebApi.Filters.Exception;
using CQ.Extensions.ServiceCollection;
using CQ.IdentityProvider.EfCore;
using CQ.IdentityProvider.EfCore.AppConfig;
using CQ.UnitOfWork.EfCore.Core;
using Microsoft.EntityFrameworkCore;

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

    public static IServiceProvider AddDbContextMissingMigrations<TContext>(
        this IServiceProvider services,
        IWebHostEnvironment environment)
        where TContext : EfCoreContext
    {
        if (environment.IsProduction() || environment.IsDevelopment())
        {
            return services;
        }

        using var scope = services.CreateScope();

        var authProviderDbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        var pendingMigrations = authProviderDbContext.Database.GetPendingMigrations();

        if (pendingMigrations.Any())
        {
            authProviderDbContext.Database.Migrate();
        }

        return services;
    }
}
