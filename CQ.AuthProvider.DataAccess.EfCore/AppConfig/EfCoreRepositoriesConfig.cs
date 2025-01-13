﻿using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Invitations;
using CQ.AuthProvider.BusinessLogic.Permissions;
using CQ.AuthProvider.BusinessLogic.ResetPasswords;
using CQ.AuthProvider.BusinessLogic.Roles;
using CQ.AuthProvider.BusinessLogic.Sessions;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.AuthProvider.DataAccess.EfCore.Accounts;
using CQ.AuthProvider.DataAccess.EfCore.Apps;
using CQ.AuthProvider.DataAccess.EfCore.Invitations;
using CQ.AuthProvider.DataAccess.EfCore.Permissions;
using CQ.AuthProvider.DataAccess.EfCore.ResetPasswords;
using CQ.AuthProvider.DataAccess.EfCore.Roles;
using CQ.AuthProvider.DataAccess.EfCore.Sessions;
using CQ.AuthProvider.DataAccess.EfCore.Tenants;
using CQ.Extensions.Configuration;
using CQ.Extensions.ServiceCollection;
using CQ.UnitOfWork.EfCore.Configuration;
using CQ.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Composition.Hosting.Core;

namespace CQ.AuthProvider.DataAccess.EfCore.AppConfig;

public static class EfCoreRepositoriesConfig
{
    public static IServiceCollection ConfigureDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddMappings()
            .AddRepositories(configuration)
            ;

        return services;
    }

    private static IServiceCollection AddMappings(this IServiceCollection services)
    {
        services
            .AddKeyedTransient<IMapper>(MapperKeyedService.DataAccess, (provider, _) =>
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
                    config.AddProfile<SessionProfile>();
                });

                return config.CreateMapper();
            });

        return services;
    }

    private static IServiceCollection AddRepositories(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Auth");
        Guard.ThrowIsNullOrEmpty(connectionString, "ConnectionStrings:Auth");

        var databaseEngine = configuration.GetSection<string>("DatabaseEngine");

        var optionsBuilder = databaseEngine switch
        {
            "Sql" => (DbContextOptionsBuilder options) =>
            options
            .UseSqlServer(connectionString),
            "Postgres" => (DbContextOptionsBuilder options) =>
            options
            .Usepos

            _ => throw new InvalidOperationException("Invalid value of DatabaseEngine")
        };


        services
            .AddContext(optionsBuilder, LifeTime.Scoped)
            .AddUnitOfWork<AuthDbContext>(LifeTime.Scoped)

            .AddAbstractionRepository<AccountEfCore, IAccountRepository, AccountRepository>(LifeTime.Scoped)
            .AddAbstractionRepository<RoleEfCore, IRoleRepository, RoleRepository>(LifeTime.Scoped)
            .AddRepositoryForContext<RolePermission, AuthDbContext>(LifeTime.Scoped)
            .AddAbstractionRepository<PermissionEfCore, IPermissionRepository, PermissionRepository>(LifeTime.Scoped)
            .AddAbstractionRepository<SessionEfCore, ISessionRepository, SessionRepository>(LifeTime.Scoped)
            .AddAbstractionRepository<ResetPasswordEfCore, IResetPasswordRepository, ResetPasswordRepository>(LifeTime.Scoped)
            .AddAbstractionRepository<InvitationEfCore, IInvitationRepository, InvitationRepository>(LifeTime.Scoped)
            .AddAbstractionRepository<AppEfCore, IAppRepository, AppRepository>(LifeTime.Scoped)
            .AddAbstractionRepository<TenantEfCore, ITenantRepository, TenantRepository>(LifeTime.Scoped)
            ;

        return services;
    }
}
