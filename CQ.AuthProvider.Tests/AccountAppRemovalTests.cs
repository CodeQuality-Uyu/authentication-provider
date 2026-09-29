using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Accounts.Exceptions;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.GoogleAuth.Exceptions;
using CQ.AuthProvider.BusinessLogic.Utils;

namespace CQ.AuthProvider.Tests;

/// <summary>
/// La regla común de <c>DELETE /me</c> y <c>DELETE /accounts/{id}</c>: qué implica sacar una
/// cuenta de un app.
/// </summary>
public sealed class AccountAppRemovalTests
{
    private static readonly Guid AppId = Guid.NewGuid();

    private static readonly Guid OtherAppId = Guid.NewGuid();

    [Fact]
    public void An_account_in_other_apps_is_only_removed_from_this_one()
    {
        var account = AccountIn(AppId, OtherAppId);

        Assert.Equal(AppRemoval.RemoveApp, account.ResolveRemovalFrom(AppId));
    }

    [Fact]
    public void An_account_whose_only_app_is_this_one_is_deleted()
    {
        var account = AccountIn(AppId);

        Assert.Equal(AppRemoval.DeleteAccount, account.ResolveRemovalFrom(AppId));
    }

    [Fact]
    public void An_account_can_not_be_removed_from_the_auth_provider_console()
    {
        var account = AccountIn(AuthConstants.AUTH_WEB_API_APP_ID, AppId);

        Assert.Throws<AccountDeletionNotAllowedException>(
            () => account.ResolveRemovalFrom(AuthConstants.AUTH_WEB_API_APP_ID));
    }

    [Fact]
    public void An_account_can_not_be_removed_from_an_app_it_does_not_belong_to()
    {
        // Sin este chequeo, una cuenta de una sola app ajena se borraría entera: no tiene "otras
        // apps" que la salven.
        var account = AccountIn(OtherAppId);

        Assert.Throws<AccountNotInAppException>(() => account.ResolveRemovalFrom(AppId));
    }

    private static Account AccountIn(params Guid[] appIds)
    {
        return new Account
        {
            Email = "account@test.com",
            Apps = [.. appIds.Select(id => new App { Id = id })],
        };
    }
}
