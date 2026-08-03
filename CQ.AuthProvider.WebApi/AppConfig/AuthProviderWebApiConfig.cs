using System.Reflection;
using Amazon.Runtime;
using Amazon.S3;
using AutoMapper;
using CQ.ApiElements.AppConfig;
using CQ.AuthProvider.BusinessLogic.AppConfig;
using CQ.AuthProvider.BusinessLogic.Blobs;
using CQ.AuthProvider.BusinessLogic.GoogleAuth;
using CQ.AuthProvider.BusinessLogic.Sessions;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.AuthProvider.WebApi.Sessions;
using CQ.AuthProvider.WebApi.GoogleAuth;
using CQ.AuthProvider.DataAccess.EfCore;
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
using CQ.AuthProvider.WebApi.Healths;
using CQ.Extensions.Configuration;
using CQ.Extensions.ServiceCollection;
using CQ.IdentityProvider.EfCore;
using CQ.IdentityProvider.EfCore.AppConfig;
using CQ.UnitOfWork.EfCore.Core;
using CQ.Utility;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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

            .ConfigureSwagger()

            .ConfigureServices()

            .AddAccountDataEnrichment()

            .AddGoogleAuth()

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

    private static IServiceCollection ConfigureSwagger(
        this IServiceCollection services)
    {
        services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CQ Auth Provider Web API",
                    Version = "v1",
                    Description = "API documentation"
                });

                var xmlName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlName);
                if (File.Exists(xmlPath))
                {
                    o.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }

                o.CustomSchemaIds(t => t.FullName);
            });

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
                    config.ConstructServicesUsing(provider.GetRequiredService);

                    config.AddProfile<MeProfile>();
                    config.AddProfile<PermissionProfile>();
                    config.AddProfile<RoleProfile>();
                    config.AddProfile<InvitationProfile>();
                    config.AddProfile<SessionMapping>();
                    config.AddProfile<AppProfile>();
                    config.AddProfile<AccountProfile>();
                    config.AddProfile<TenantMapping>();
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

    private static IServiceCollection AddAccountDataEnrichment(
        this IServiceCollection services)
    {
        services
            .AddScoped<IAccountDataEnricher, HttpAccountDataEnricher>();

        return services;
    }

    private static IServiceCollection AddGoogleAuth(
        this IServiceCollection services)
    {
        services
            .AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();

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
        if (environment.IsProduction())
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

#region Blob Configuration Section
    public static IServiceCollection ConfigureBlob(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var blobConfiguration = configuration.GetSection<BlobSection>("Blob");
        switch (blobConfiguration.Type)
        {
            case BlobType.Mock:
                {
                    ConfigureMockBlob(services);
                    break;
                }
            case BlobType.Aws:
                {
                    ConfigureAwsBlob(services, configuration);
                    break;
                }
            case BlobType.LocalStack:
                {
                    ConfigureLocalStackBlob(services, configuration);
                    break;
                }
            default:
                throw new InvalidOperationException($"Invalid blob type: {blobConfiguration.Type}");
        }

        services.Configure<BlobSection>(configuration.GetSection("Blob"));

        return services;
    }

    private static IServiceCollection ConfigureMockBlob(
        IServiceCollection services)
    {
        services.AddTransient<IBlobService, FakeBlobService>();

        return services;
    }

    private static IServiceCollection ConfigureAwsBlob(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDefaultAWSOptions(configuration.GetAWSOptions("AWS"))
            .AddAWSService<AmazonS3Client>()
            .AddSingleton<IAmazonS3>(sp => sp.GetRequiredService<AmazonS3Client>())
            .AddTransient<IBlobService, AWSBlobService>();
            
        return services;
    }

    private static IServiceCollection ConfigureLocalStackBlob(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var localStack = configuration.GetSection<LocalStackSection>("LocalStack");
        var credentials = new BasicAWSCredentials(localStack.AccessToken, localStack.SecretToken);
        var config = new AmazonS3Config
        {
            ServiceURL = localStack.ServiceUrl,
            ForcePathStyle = true,
        };

        var client = new AmazonS3Client(credentials, config);

        services
            .AddTransient<IBlobService, AWSBlobService>()
            .AddService(client, LifeTime.Singleton);

        return services;
    }
#endregion Blob Configuration Section

    public static IServiceCollection ConfigHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authConnectionString = configuration.GetConnectionString("Auth");
        AssertConnectionString(authConnectionString);

        var identityConnectionString = configuration.GetConnectionString("Identity");
        AssertConnectionString(identityConnectionString);

        var databaseEngineAuth = configuration.GetSection<string>($"DatabaseEngine:Auth");
        var databaseEngineIdentity = configuration.GetSection<string>($"DatabaseEngine:Identity");

        services.Configure<VersionSection>(configuration.GetSection("Version"));

        var healthCheck = services
            .AddHealthChecks()
            .AddCheck<VersionHealthCheck>("Version", tags: ["version"]);

        if (databaseEngineAuth == DatabaseEngineOption.Sql)
        {
            healthCheck = healthCheck
                .AddSqlServer(
                    authConnectionString!,
                    name: "AuthSQLServer",
                    tags: ["db", "auth", "sqlserver"]);
        }
        else if (databaseEngineAuth == DatabaseEngineOption.Postgres)
        {
            healthCheck = healthCheck
                .AddNpgSql(
                    authConnectionString!,
                    name: "AuthPostgreSQL",
                    tags: ["db", "auth", "postgresql"]);
        }

        if (databaseEngineIdentity == DatabaseEngineOption.Sql)
        {
            healthCheck = healthCheck
                .AddSqlServer(
                    identityConnectionString!,
                    name: "IdentitySQLServer",
                    tags: ["db", "identity", "sqlserver"]);
        }
        else if (databaseEngineIdentity == DatabaseEngineOption.Postgres)
        {
            healthCheck = healthCheck
                .AddNpgSql(
                    identityConnectionString!,
                    name: "IdentityPostgreSQL",
                    tags: ["db", "identity", "postgresql"]);
        }

        services
        .AddHealthChecks()
        .AddDbContextCheck<AuthDbContext>("AuthDatabase", tags: ["db", "efcore"])
        .AddDbContextCheck<IdentityDbContext>("IdentityDatabase", tags: ["db", "efcore"]);

        return services;
    }

    private static void AssertConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The connection string 'DefaultConnection' is not configured.");
        }
    }
}
