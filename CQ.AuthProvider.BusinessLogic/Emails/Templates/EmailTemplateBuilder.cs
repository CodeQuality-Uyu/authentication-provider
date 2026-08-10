using System.Globalization;
using System.Net;
using System.Text;

namespace CQ.AuthProvider.BusinessLogic.Emails.Templates;

/// <summary>
/// Builds the raw HTML (inline styles only — mail clients don't reliably support a
/// <c>&lt;style&gt;</c> block) for the three account mails this provider sends. Branding
/// (<c>LogoUrl</c>) and links are already resolved by the caller; this class only knows how to
/// lay them out.
/// </summary>
internal sealed class EmailTemplateBuilder : IEmailTemplateBuilder
{
    private const string AccentColor = "#5B4FE9";
    private const string HeadingColor = "#170F49";
    private const string BodyColor = "#5F696E";

    public string BuildResetPassword(BuildResetPasswordArgs args) =>
        BuildLayout(
            logoUrl: args.LogoUrl,
            heading: "Restablecé tu contraseña",
            subheading: null,
            bodyHtml: """
                <p style="font-size:13px;color:#5F696E;line-height:1.6;margin:0;">
                  Recibimos un pedido para restablecer tu contraseña. Ingresá el siguiente código para continuar.
                </p>
                """,
            code: args.Code,
            ctaUrl: null,
            ctaLabel: null,
            reassuranceHtml: """
                <p style="font-size:12px;color:#5F696E;line-height:1.6;margin:0;">
                  Si no solicitaste este cambio, podés ignorar este correo — tu contraseña actual sigue siendo válida.
                </p>
                """);

    public string BuildInviteUser(BuildInviteUserArgs args)
    {
        var creatorName = WebUtility.HtmlEncode(args.CreatorName);

        return BuildLayout(
            logoUrl: args.LogoUrl,
            heading: "Te invitaron a unirte",
            subheading: null,
            bodyHtml: $"""
                <p style="font-size:13px;color:#5F696E;line-height:1.6;margin:0;">
                  <strong>{creatorName}</strong> te invitó a sumarte a la plataforma. Ingresá el siguiente código para aceptar la invitación.
                </p>
                """,
            code: args.Code,
            ctaUrl: null,
            ctaLabel: null,
            reassuranceHtml: """
                <p style="font-size:12px;color:#5F696E;line-height:1.6;margin:0;">
                  Si no esperabas esta invitación, podés ignorar este correo.
                </p>
                """);
    }

    public string BuildEmailVerification(BuildEmailVerificationArgs args)
    {
        var hasLink = args.VerificationUrl is not null;

        var ctaLabel = hasLink ? "Verificar mi cuenta" : null;
        var bodyHtml = hasLink
            ? """
                <p style="font-size:13px;color:#5F696E;line-height:1.6;margin:0;">
                  Para terminar de crear tu cuenta, hacé clic en el siguiente botón o ingresá el código a continuación.
                </p>
                """
            : """
                <p style="font-size:13px;color:#5F696E;line-height:1.6;margin:0;">
                  Para terminar de crear tu cuenta, ingresá el siguiente código.
                </p>
                """;

        return BuildLayout(
            logoUrl: args.LogoUrl,
            heading: "Confirmá tu cuenta",
            subheading: "Verificá tu identidad",
            bodyHtml: bodyHtml,
            code: args.Code,
            ctaUrl: args.VerificationUrl,
            ctaLabel: ctaLabel,
            reassuranceHtml: """
                <p style="font-size:12px;color:#5F696E;line-height:1.6;margin:0;">
                  Si no creaste esta cuenta, podés ignorar este correo.
                </p>
                """);
    }

    private static string BuildLayout(
        string? logoUrl,
        string heading,
        string? subheading,
        string bodyHtml,
        int code,
        string? ctaUrl,
        string? ctaLabel,
        string reassuranceHtml)
    {
        var logoHtml = BuildLogoHtml(logoUrl);
        var subheadingHtml = subheading is not null
            ? $"<h2 style=\"font-size:15px;color:{HeadingColor};margin:4px 0 0;font-weight:normal;\">{WebUtility.HtmlEncode(subheading)}</h2>"
            : string.Empty;
        var ctaHtml = BuildCtaHtml(ctaUrl, ctaLabel);
        var codeBoxesHtml = BuildCodeBoxesHtml(code);

        return $"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
              <meta charset="UTF-8">
              <meta name="viewport" content="width=device-width,initial-scale=1.0">
            </head>
            <body style="margin:0;padding:0;background-color:#f0f0f0;font-family:Arial,Helvetica,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0">
                <tr>
                  <td align="center" style="padding:30px 0;">
                    <table width="480" cellpadding="0" cellspacing="0" style="background-color:#ffffff;border-radius:6px;">
                      <tr>
                        <td align="center" style="padding:30px 40px 12px;">
                          {logoHtml}
                        </td>
                      </tr>
                      <tr>
                        <td align="center" style="padding:0 40px 4px;">
                          <h1 style="font-size:20px;color:{HeadingColor};margin:0;">{WebUtility.HtmlEncode(heading)}</h1>
                          {subheadingHtml}
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:16px 40px 20px;">
                          {bodyHtml}
                        </td>
                      </tr>
                      {ctaHtml}
                      <tr>
                        <td align="center" style="padding:0 40px 8px;">
                          <p style="font-size:11px;font-weight:bold;color:{BodyColor};text-transform:uppercase;letter-spacing:0.5px;margin:0 0 12px;">
                            {(ctaHtml.Length > 0 ? "O ingresá el código" : "Tu código de verificación")}
                          </p>
                          <table cellpadding="0" cellspacing="0">
                            <tr>
                              {codeBoxesHtml}
                            </tr>
                          </table>
                        </td>
                      </tr>
                      <tr>
                        <td align="center" style="padding:16px 40px 30px;border-top:1px solid #f0f0f0;margin-top:16px;">
                          {reassuranceHtml}
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }

    private static string BuildLogoHtml(string? logoUrl) =>
        logoUrl is not null
            ? $"<img src=\"{WebUtility.HtmlEncode(logoUrl)}\" height=\"40\" style=\"display:block;\" alt=\"Logo\">"
            : string.Empty;

    private static string BuildCtaHtml(string? ctaUrl, string? ctaLabel)
    {
        if (ctaUrl is null || ctaLabel is null)
        {
            return string.Empty;
        }

        var url = WebUtility.HtmlEncode(ctaUrl);
        var label = WebUtility.HtmlEncode(ctaLabel);

        return $"""
            <tr>
              <td align="center" style="padding:0 40px 24px;">
                <a href="{url}" style="display:inline-block;background-color:{AccentColor};color:#ffffff;font-size:13px;font-weight:bold;text-decoration:none;padding:14px 28px;border-radius:6px;">{label}</a>
              </td>
            </tr>
            """;
    }

    private static string BuildCodeBoxesHtml(int code)
    {
        var digits = code.ToString(CultureInfo.InvariantCulture);
        var sb = new StringBuilder();

        foreach (var digit in digits)
        {
            sb.Append($"""
                <td width="36" align="center" style="padding:0 4px;">
                  <div style="background-color:{AccentColor};color:#ffffff;font-size:18px;font-weight:bold;border-radius:6px;padding:12px 0;">{digit}</div>
                </td>
                """);
        }

        return sb.ToString();
    }
}
