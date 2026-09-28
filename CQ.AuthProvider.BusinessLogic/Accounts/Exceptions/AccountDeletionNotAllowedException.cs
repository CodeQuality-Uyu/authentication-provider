namespace CQ.AuthProvider.BusinessLogic.Accounts.Exceptions;

/// <summary>
/// The account tried to delete itself while logged into the Auth Provider Web API console. That
/// app is where tenants and apps are administered, so leaving it could orphan them; the account
/// has to delete itself from the client app it belongs to instead.
/// </summary>
public sealed class AccountDeletionNotAllowedException(
    string email,
    Guid appId)
    : Exception
{
    public string Email { get; } = email;

    public Guid AppId { get; } = appId;
}
