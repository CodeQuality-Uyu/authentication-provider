namespace CQ.AuthProvider.BusinessLogic.Apps;

/// <summary>
/// Se intentó actuar sobre una app que no es la logueada sin el alcance necesario (ver
/// <see cref="Accounts.AccountLogged.AssertCanReachApp"/>).
/// </summary>
public sealed class CrossAppAccessDeniedException(
    Guid appId,
    string requirement)
    : Exception
{
    public Guid AppId { get; } = appId;

    /// <summary>Qué le falta a quien llama, para el mensaje de error.</summary>
    public string Requirement { get; } = requirement;
}
