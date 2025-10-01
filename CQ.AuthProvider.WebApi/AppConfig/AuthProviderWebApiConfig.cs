using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using AutoMapper;
using CQ.ApiElements.AppConfig;
using CQ.AuthProvider.BusinessLogic.AppConfig;
using CQ.AuthProvider.BusinessLogic.Blobs;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.AuthProvider.DataAccess.EfCore.AppConfig;
using CQ.AuthProvider.Postgres.Migrations;
using CQ.AuthProvider.Sql.Migrations;
using CQ.AuthProvider.WebApi.Controllers.Accounts;
using CQ.AuthProvider.WebApi.Controllers.Apps;
using CQ.AuthProvider.WebApi.Controllers.Blobs;
using CQ.AuthProvider.WebApi.Controllers.Invitations;
using CQ.AuthProvider.WebApi.Controllers.Me;
using CQ.AuthProvider.WebApi.Controllers.Permissions;
using CQ.AuthProvider.WebApi.Controllers.Roles;
using CQ.AuthProvider.WebApi.Controllers.Sessions;
using CQ.AuthProvider.WebApi.Controllers.Tenants;
using CQ.AuthProvider.WebApi.Filters;
using CQ.Extensions.Configuration;
using CQ.Extensions.Environments;
using CQ.Extensions.ServiceCollection;
using CQ.IdentityProvider.EfCore.AppConfig;
using CQ.UnitOfWork.EfCore.Core;
using CQ.Utility;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace CQ.AuthProvider.WebApi.AppConfig;

internal static class AuthProviderWebApiConfig
{
    public static IServiceCollection ConfigureAutoValidation(
        this IServiceCollection services)
    {
        services
            .AddFluentValidationAutoValidation(configuration =>
        {
            // Disable the built-in .NET model (data annotations) validation.
            configuration.DisableBuiltInModelValidation = true;

            // Only validate controllers decorated with the `FluentValidationAutoValidation` attribute.
            //configuration.ValidationStrategy = ValidationStrategy.Annotations;

            // Enable validation for parameters bound from `BindingSource.Body` binding sources.
            //configuration.EnableBodyBindingSourceAutomaticValidation = true;

            // Enable validation for parameters bound from `BindingSource.Form` binding sources.
            configuration.EnableFormBindingSourceAutomaticValidation = true;

            // Enable validation for parameters bound from `BindingSource.Query` binding sources.
            //configuration.EnableQueryBindingSourceAutomaticValidation = true;

            // Enable validation for parameters bound from `BindingSource.Path` binding sources.
            configuration.EnablePathBindingSourceAutomaticValidation = true;

            // Enable validation for parameters bound from 'BindingSource.Custom' binding sources.
            configuration.EnableCustomBindingSourceAutomaticValidation = true;

            // Replace the default result factory with a custom implementation.
            configuration.OverrideDefaultResultFactoryWith<CQBadRequestResponseFactory>();
        })
            .AddTransient<IValidator<CreateBlobRequest>, CreateBlobRequestValidator>()
            ;

        return services;
    }

    public static IServiceCollection ConfigureApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddExceptionGlobalHandlerService<CQAuthExceptionRegistryService>(LifeTime.Transient)

            .AddMappings()

            .ConfigureServices()

            .ConfigureDbContext(
            configuration,
            "Auth",
            services.ConfigureAuthProviderSql,
            services.ConfigureAuthProviderPostgres)

            .ConfigureDataAccess(configuration)

            .ConfigureLocalIdentityProvider(configuration)

            .AddFakeAuthentication<FakeAccountLogged>(configuration, environment, fakeAuthenticationLifeTime: LifeTime.Transient)

            .Configure<DatabaseEngineSection>(configuration.GetSection(DatabaseEngineSection.SectionName))
            ;

        return services;
    }

    private static IServiceCollection AddMappings(
        this IServiceCollection services)
    {
        services
            .AddAutoMapper(typeof(Program))
            .AddKeyedTransient<IMapper>(MapperKeyedService.Presentation, (provider, _) =>
            {
                var config = new MapperConfiguration(config =>
                {
                    config.ConstructServicesUsing(provider.GetService);

                    config.AddProfile<MeProfile>();
                    config.AddProfile<PermissionProfile>();
                    config.AddProfile<RoleProfile>();
                    config.AddProfile<InvitationProfile>();
                    config.AddProfile<SessionProfile>();
                    config.AddProfile<AppProfile>();
                    config.AddProfile<AccountProfile>();
                    config.AddProfile<TenantProfile>();
                });

                return config.CreateMapper();
            });

        return services;
    }

    private static IServiceCollection ConfigureLocalIdentityProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseEngine = configuration.GetSection<string>("DatabaseEngine:Identity");

        if (databaseEngine == DatabaseEngineOption.Sql || databaseEngine == DatabaseEngineOption.Postgres)
        {
            services
                .ConfigureDbContext(
                configuration,
                "Identity",
                services.ConfigureIdentityProviderSql,
                services.ConfigureIdentityProviderPostgres)
                .ConfigureIdentityProvider(configuration)
                ;
        }

        return services;
    }

    private static IServiceCollection ConfigureDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringKey,
        Func<string, IServiceCollection> ConfigureSql,
        Func<string, IServiceCollection> ConfigurePostgres)
    {
        var connectionString = configuration.GetConnectionString(connectionStringKey);
        Guard.ThrowIsNullOrEmpty(connectionString, $"ConnectionStrings:{connectionStringKey}");

        var databaseEngine = configuration.GetSection<string>($"DatabaseEngine:{connectionStringKey}");

        services = databaseEngine switch
        {
            DatabaseEngineOption.Sql => ConfigureSql(connectionString!),

            DatabaseEngineOption.Postgres => ConfigurePostgres(connectionString!),

            _ => throw new InvalidOperationException($"Invalid value of DatabaseEngine:{connectionStringKey}")
        };

        return services;
    }

    public static IServiceProvider AddDbContextMissingMigrations<TContext>(
        this IServiceProvider services,
        IWebHostEnvironment environment)
        where TContext : EfCoreContext
    {
        if (environment.IsProd())
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

    public static IServiceCollection ConfigureBlob(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var blobConfiguration = configuration.GetSection<BlobSection>("Blob");

        if(blobConfiguration.Type == BlobType.Mock)
        {
            services.AddTransient<IBlobService, FakeBlobService>();
        }
        else
        {
            var credentials = new BasicAWSCredentials(blobConfiguration.Config.AccessToken, blobConfiguration.Config.SecretToken);
            AmazonS3Client client;

            if (blobConfiguration.Type == BlobType.Aws)
            {
                var region = RegionEndpoint.GetBySystemName(blobConfiguration.Config.Region);

                client = new AmazonS3Client(credentials, region);
            }
            else
            {
                var fakeConfig = new AmazonS3Config
                {
                    ServiceURL = blobConfiguration.Config.ServiceUrl,
                    ForcePathStyle = true,
                };

                client = new AmazonS3Client(credentials, fakeConfig);
            }

            services
                .AddTransient<IBlobService, BlobService>()
                .AddService<IAmazonS3>(client, LifeTime.Singleton);
        }

        return services;
    }
}
