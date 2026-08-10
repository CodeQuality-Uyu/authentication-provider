using CQ.AuthProvider.WebApi.AppConfig;
using CQ.ApiElements.AppConfig;
using CQ.AuthProvider.DataAccess.EfCore;
using CQ.IdentityProvider.EfCore;
using CQ.Extensions.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

// Config controllers
services
    .AddControllers(
    (options) =>
    {
        options.AddExceptionGlobalHandler();
    });

// Enums travel as their name rather than as the ordinal, but that is declared
// per type with [JsonConverter] — see TokenFormat. Registering the converter
// globally here would also catch ErrorResponse.StatusCode, turning the
// "statusCode" of every error from 401 into "Unauthorized" and breaking anyone
// parsing it as a number.

// Add services to the container.
services
    .ConfigureAutoValidation()
    .ConfigureApiServices(configuration, builder.Environment)
    .ConfigureBlob(configuration, builder.Environment)
    .ConfigHealthChecks(configuration);

var app = builder.Build();

// Config missing migrations
app
    .Services
    .AddDbContextMissingMigrations<AuthDbContext>(app.Environment)
    .AddDbContextMissingMigrations<IdentityDbContext>(app.Environment);

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

var allowedOrigins = app.Configuration.GetSection<List<string>>("Cors:Origins") ?? [];
if (allowedOrigins.Count == 0 && !app.Environment.IsProduction())
{
    allowedOrigins.AddRange(["http://localhost:3000", "https://localhost:3000"]);
}

app.UseCors(policy => policy.WithOrigins(allowedOrigins.ToArray()).AllowAnyHeader().AllowAnyMethod());
app.MapHealthChecks(
    "health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

ConfigureSwagger(app);

app.MapControllers();

app.Run();

static void ConfigureSwagger(WebApplication app)
{
    if (app.Environment.IsProduction())
    {
        return;
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

