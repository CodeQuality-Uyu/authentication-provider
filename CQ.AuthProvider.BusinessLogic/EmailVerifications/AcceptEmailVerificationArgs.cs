namespace CQ.AuthProvider.BusinessLogic.EmailVerifications;

public sealed record AcceptEmailVerificationArgs(
    string Email,
    string? Token,
    int? Code);
