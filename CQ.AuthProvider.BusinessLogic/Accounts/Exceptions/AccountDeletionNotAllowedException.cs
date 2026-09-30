namespace CQ.AuthProvider.BusinessLogic.Accounts.Exceptions;

/// <summary>
/// An account was about to be removed from the Auth Provider Web API console, either by itself
/// (<c>DELETE /me</c>) or by an admin logged into it (<c>DELETE /accounts/{id}</c>). That app is
/// where tenants and apps are administered, so leaving it could orphan them; the removal has to
/// happen from the client app the account belongs to instead.
/// </summary>
public sealed class AccountDeletionNotAllowedException(
    string email,
    Guid appId)
    : Exception
{
    public string Email { get; } = email;

    public Guid AppId { get; } = appId;
}
