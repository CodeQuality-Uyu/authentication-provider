namespace CQ.AuthProvider.BusinessLogic.AppConfig;

/// <summary>
/// Config del servicio compartido de envío de mail (mailing-api). Este repo solo compone el
/// email (asunto, HTML) y le pega a este endpoint para el envío real — no sabe con qué proveedor
/// (Resend, etc.) termina saliendo, eso lo decide mailing-api.
/// </summary>
public sealed record MailingApiSection
{
    public string BaseUrl { get; init; } = null!;

    /// <summary>
    /// Enviado como header <c>X-Api-Key</c>. Tiene que coincidir con la entrada
    /// <c>"authentication-provider"</c> de <c>Security:ApiKeys</c> en mailing-api — ese servicio
    /// rechaza (401) cualquier request sin una key configurada que matchee.
    /// </summary>
    public string? ApiKey { get; init; }
}
