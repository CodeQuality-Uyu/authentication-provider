namespace CQ.AuthProvider.BusinessLogic.Emails.MailingApi;

/// <summary>
/// Payload sent as-is to the shared mailing-api's <c>POST /mailing/send</c>. Mirrors that
/// service's own <c>SendMailArgs</c> contract — it only knows how to dispatch an
/// already-composed mail, it doesn't know anything about accounts/tenants/templates.
/// </summary>
internal sealed record SendMailArgs(
    string From,
    string To,
    string Subject,
    string Html
);
