namespace CQ.AuthProvider.BusinessLogic.Tenants;

/// <summary>
/// Se pidió filtrar por un tenant que no es el de la cuenta logueada sin tener la vista global
/// (<c>getallcrosstenant-account</c>).
/// </summary>
/// <remarks>
/// Se responde 403 en vez de ignorar el <c>tenantId</c> en silencio: un filtro ignorado devuelve
/// datos de otro alcance sin que el cliente se entere (es lo que pasa hoy con el <c>appId</c> de
/// <c>GET /roles</c>).
/// </remarks>
public sealed class CrossTenantAccessDeniedException(Guid tenantId)
    : Exception
{
    public Guid TenantId { get; } = tenantId;
}
