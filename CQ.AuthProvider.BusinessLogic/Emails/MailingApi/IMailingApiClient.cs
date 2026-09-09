namespace CQ.AuthProvider.BusinessLogic.Emails.MailingApi;

internal interface IMailingApiClient
{
    Task SendAsync(SendMailArgs args);
}
