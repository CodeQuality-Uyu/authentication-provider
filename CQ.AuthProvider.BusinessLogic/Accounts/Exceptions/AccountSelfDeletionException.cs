namespace CQ.AuthProvider.BusinessLogic.Accounts.Exceptions;

/// <summary>
/// La cuenta intentó sacarse a sí misma por <c>DELETE /accounts/{id}</c>. Para eso está
/// <c>DELETE /me</c>: el endpoint de admin es para sacar a otra cuenta, y así un admin no se
/// deja afuera del panel por error.
/// </summary>
public sealed class AccountSelfDeletionException(Guid accountId)
    : Exception
{
    public Guid AccountId { get; } = accountId;
}
