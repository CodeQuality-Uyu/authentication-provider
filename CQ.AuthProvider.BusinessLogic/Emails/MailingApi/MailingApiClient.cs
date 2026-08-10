using CQ.AuthProvider.BusinessLogic.AppConfig;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CQ.AuthProvider.BusinessLogic.Emails.MailingApi;

internal sealed class MailingApiClient(
    IOptions<MailingApiSection> mailingApiOptions,
    ILogger<MailingApiClient> logger) : IMailingApiClient
{
    private const int TimeoutSeconds = 20;

    private readonly MailingApiSection _mailingApi = mailingApiOptions.Value;

    public async Task SendAsync(SendMailArgs args)
    {
        var request = _mailingApi.BaseUrl
            .AppendPathSegment("mailing/send")
            .WithTimeout(TimeSpan.FromSeconds(TimeoutSeconds));

        if (!string.IsNullOrWhiteSpace(_mailingApi.ApiKey))
        {
            request = request.WithHeader("X-Api-Key", _mailingApi.ApiKey);
        }

        var response = await request
            .AllowAnyHttpStatus()
            .PostJsonAsync(args)
            .ConfigureAwait(false);

        if (response.StatusCode is < 200 or >= 300)
        {
            var body = await response
                .GetStringAsync()
                .ConfigureAwait(false);

            logger.LogWarning(
                "Mailing API returned a non-success status. StatusCode={StatusCode} To={To} Body={Body}",
                response.StatusCode,
                args.To,
                body);

            // Intencional: a diferencia de los mails informativos de pedidos, acá el mail ES el
            // mecanismo (código de verificación/reset/invitación). Si mailing-api falla, el caller
            // tiene que enterarse — devolver éxito acá dejaría al usuario sin forma de completar
            // el flujo y sin ningún error visible.
            throw new InvalidOperationException(
                $"Mailing API request failed with status code {response.StatusCode}.");
        }
    }
}
