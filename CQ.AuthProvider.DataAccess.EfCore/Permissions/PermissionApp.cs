using CQ.AuthProvider.DataAccess.EfCore.Apps;

namespace CQ.AuthProvider.DataAccess.EfCore.Permissions;

/// <summary>
/// Grant explícito de un permiso a una app, dentro del mismo tenant.
/// </summary>
/// <remarks>
/// No reemplaza a <see cref="PermissionEfCore.AppId"/>, que sigue siendo la app <b>dueña</b> del
/// permiso. Estas filas son el alcance <b>extra</b>: la vía para compartir un permiso privado con
/// una app hija puntual, sin marcarlo público y que lo hereden todas.
/// </remarks>
public sealed record class PermissionApp()
{
    public required Guid PermissionId { get; init; }

    public PermissionEfCore Permission { get; init; } = null!;

    public required Guid AppId { get; init; }

    public AppEfCore App { get; init; } = null!;
}
