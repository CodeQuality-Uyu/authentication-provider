using CQ.AuthProvider.DataAccess.EfCore.Apps;

namespace CQ.AuthProvider.DataAccess.EfCore.Roles;

/// <summary>
/// Grant explícito de un rol a una app, dentro del mismo tenant.
/// </summary>
/// <remarks>
/// No reemplaza a <see cref="RoleEfCore.AppId"/>, que sigue siendo la app <b>dueña</b> del rol
/// (la raíz de la herencia y el scope de unicidad del nombre). Estas filas son el alcance
/// <b>extra</b>: la vía para compartir un rol privado con una app hija puntual, sin marcarlo
/// público y que lo hereden todas.
/// </remarks>
public sealed record class RoleApp()
{
    public required Guid RoleId { get; init; }

    public RoleEfCore Role { get; init; } = null!;

    public required Guid AppId { get; init; }

    public AppEfCore App { get; init; } = null!;
}
