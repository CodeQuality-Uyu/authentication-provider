﻿using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Emails;
using CQ.AuthProvider.BusinessLogic.Invitations;
using CQ.AuthProvider.BusinessLogic.Me;
using CQ.AuthProvider.BusinessLogic.Permissions;
using CQ.AuthProvider.BusinessLogic.Roles;
using CQ.AuthProvider.BusinessLogic.Sessions;
using CQ.AuthProvider.BusinessLogic.Tenants;
using CQ.AuthProvider.BusinessLogic.Tokens;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.AuthProvider.BusinessLogic.AppConfig;

public static class ServicesConfig
{
    public static IServiceCollection ConfigureServices(
        this IServiceCollection services)
    {
        services
            .AddScoped<IAccountService, AccountService>()
            .AddScoped<IAccountInternalService, AccountService>()

            .AddScoped<ISessionService, SessionService>()
            .AddScoped<ISessionInternalService, SessionService>()

            .AddScoped<IRoleService, RoleService>()
            .AddScoped<IRoleInternalService, RoleService>()

            .AddScoped<IPermissionService, PermissionService>()
            .AddScoped<IPermissionInternalService, PermissionService>()

            .AddScoped<IAppService, AppService>()
            .AddScoped<IAppInternalService, AppService>()

            .AddScoped<IInvitationService, InvitationService>()

            .AddScoped<ITokenService, GuidTokenService>()
            .AddScoped<IEmailService, EmailService>()

            .AddScoped<IMeService, MeService>()

            .AddScoped<ITenantService, TenantService>()
            ;

        return services;
    }
}
