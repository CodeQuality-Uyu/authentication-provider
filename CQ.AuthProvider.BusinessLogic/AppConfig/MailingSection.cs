namespace CQ.AuthProvider.BusinessLogic.AppConfig;

public sealed record MailingSection
{
    public string FromEmail { get; init; } = null!;

    public string FromName { get; init; } = null!;
}
