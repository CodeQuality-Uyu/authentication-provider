namespace CQ.AuthProvider.BusinessLogic.ResetPasswords;

/// <param name="Email">Account to reset the password of.</param>
/// <param name="AppId">
/// App the reset is requested from, only to brand the mail with its logo. Optional: without it
/// (or with an app of another tenant) the mail uses the tenant's default app.
/// </param>
public sealed record CreateResetPasswordArgs(
    string Email,
    Guid? AppId = null);
