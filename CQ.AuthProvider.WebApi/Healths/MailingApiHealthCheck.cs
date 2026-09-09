using CQ.AuthProvider.BusinessLogic.AppConfig;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CQ.AuthProvider.WebApi.Healths;

/// <summary>
/// Chequea que mailing-api (del que depende para verificación de email y reset de password) esté
/// realmente arriba y accesible con la config actual de <see cref="MailingApiSection.BaseUrl"/> —
/// no alcanza con que este servicio esté "Healthy", el 401/timeout/URL mal apuntada de mailing-api
/// no se nota hasta que alguien intenta registrarse. Pega a su propio /health (sin X-Api-Key, ese
/// endpoint no pasa por el filtro de auth de mailing-api) en vez de mandar un email real.
/// </summary>
internal sealed class MailingApiHealthCheck(
    IOptions<MailingApiSection> mailingApiOptions)
    : IHealthCheck
{
    private const int TimeoutSeconds = 5;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = mailingApiOptions.Value.BaseUrl;
        var data = new Dictionary<string, object?> { { "baseUrl", baseUrl } };

        try
        {
            var response = await baseUrl
                .AppendPathSegment("health")
                .WithTimeout(TimeSpan.FromSeconds(TimeoutSeconds))
                .AllowAnyHttpStatus()
                .GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode is < 200 or >= 300)
            {
                // Degraded, no Unhealthy: que mailing-api esté caído no significa que ESTE
                // servicio no pueda atender la mayoría de sus requests (login de cuentas ya
                // verificadas sigue andando) — no tiene que tirar abajo el /health propio a 503.
                return HealthCheckResult.Degraded(
                    $"Mailing API respondió con status code {response.StatusCode}.",
                    data: data);
            }

            return HealthCheckResult.Healthy("Mailing API está accesible.", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(
                "Mailing API no está accesible.",
                exception: ex,
                data: data);
        }
    }
}
