﻿using CQ.AuthProvider.BusinessLogic.Abstractions.Identities;
using CQ.Extensions.ServiceCollection;
using CQ.IdentityProvider.EfCore.Identities;
using CQ.UnitOfWork.EfCore.Configuration;
using CQ.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.IdentityProvider.EfCore.AppConfig;

public static class EfCoreRepositoriesConfig
{
    public static IServiceCollection AddEfCoreRepositories(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Identity");
        Guard.ThrowIsNullOrEmpty(connectionString, "ConnectionStrings:Identity");

        services
            .AddContext<IdentityDbContext>(
            (options) => 
            options.UseSqlServer(connectionString),
            LifeTime.Scoped)
            .AddAbstractionRepository<Identity, IIdentityRepository, IdentityRepository>(LifeTime.Scoped)
            .AddScoped<IIdentityProviderHealthService, IdentityDbContext>();

        return services;
    }
}