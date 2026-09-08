namespace CQ.AuthProvider.BusinessLogic.EmailVerifications;

// AppId: como todavía no existe la cuenta (se verifica el email ANTES de crearla), no hay
// Account.Tenant del que sacar el branding del mail — hace falta indicar para qué app es.
public sealed record CreateEmailVerificationArgs(
    string Email,
    Guid AppId);
