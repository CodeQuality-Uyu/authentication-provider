namespace CQ.AuthProvider.BusinessLogic.Emails;

/// <summary>
/// Resolves the logo to brand account mails (reset password, invitation, email verification)
/// with — this provider is multi-tenant, so there's no single fixed identity to fall back to.
/// </summary>
internal interface IAccountEmailBrandingResolver
{
    /// <summary>
    /// Logo of the app the mail is sent from: <paramref name="appId"/> when it belongs to the
    /// tenant, otherwise the tenant's default app. Null when there's no app to take it from.
    /// </summary>
    Task<string?> GetLogoUrlAsync(Guid? appId, Guid tenantId);
}
