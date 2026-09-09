namespace CQ.AuthProvider.BusinessLogic.ResetPasswords;

public sealed record VerifyResetPasswordArgs(
    string Email,
    int Code);
