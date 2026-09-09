namespace CQ.AuthProvider.BusinessLogic.Emails;

/// <summary>
/// Resolves the logo to brand account mails (reset password, invitation, email verification)
/// with — this provider is multi-tenant, so there's no single fixed identity to fall back to.
/// </summary>
internal interface IAccountEmailBrandingResolver
{
    Task<string?> GetLogoUrlAsync(Guid tenantId);
}
