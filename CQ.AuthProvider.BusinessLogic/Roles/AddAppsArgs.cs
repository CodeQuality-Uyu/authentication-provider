namespace CQ.AuthProvider.BusinessLogic.Roles;

/// <summary>
/// Apps a las que se les da acceso explícito a un rol, dentro del mismo tenant.
/// </summary>
/// <remarks>
/// Es la vía para compartir un rol <b>privado</b> con apps hijas puntuales, sin marcarlo público
/// y que lo hereden todas las descendientes.
/// </remarks>
public sealed record AddAppsArgs(List<Guid> AppIds);
