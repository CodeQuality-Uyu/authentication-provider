namespace CQ.AuthProvider.BusinessLogic.Accounts;

// Token/Code: prueba de que el email ya se verificó (paso 2 del registro, antes de este paso 3)
// — uno de los dos, el mismo que se usó para aceptar la verificación. Sin uno de estos dos
// válido, no se puede crear la cuenta.
public sealed record CreateAccountArgs(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Locale,
    string TimeZone,
    string? ProfilePictureKey,
    Guid AppId,
    Guid? RoleId,
    string? VerificationToken = null,
    int? VerificationCode = null,
    bool IsPasswordHashed = false);

public sealed record CreateAccountForArgs(
    string Email,
    string FirstName,
    string LastName,
    string Locale,
    string TimeZone,
    string? ProfilePictureKey,
    List<Guid>? AppIds,
    List<Guid> RoleIds);

public sealed record CreateAccountWithTenantArgs(
string Email,
string Password,
string FirstName,
string LastName,
string Locale,
string TimeZone,
string? ProfilePictureKey,
string TenantName);
