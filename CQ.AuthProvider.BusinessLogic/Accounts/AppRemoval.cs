namespace CQ.AuthProvider.BusinessLogic.Accounts;

/// <summary>
/// Resultado de sacar una cuenta de un app (ver <see cref="Account.ResolveRemovalFrom"/>).
/// </summary>
public enum AppRemoval
{
    /// <summary>La cuenta usa otras apps: solo se la saca de esta.</summary>
    RemoveApp,

    /// <summary>Era su única app: se borra la cuenta entera y el email queda libre.</summary>
    DeleteAccount,
}
