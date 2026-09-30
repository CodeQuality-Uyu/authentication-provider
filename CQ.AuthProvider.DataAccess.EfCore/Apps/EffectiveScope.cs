using CQ.AuthProvider.DataAccess.EfCore.Permissions;
using CQ.AuthProvider.DataAccess.EfCore.Roles;
using Microsoft.EntityFrameworkCore;

namespace CQ.AuthProvider.DataAccess.EfCore.Apps;

/// <summary>
/// Fuente de verdad del <b>alcance efectivo</b>: qué roles y permisos aplican a una app.
/// </summary>
/// <remarks>
/// Un rol o permiso es efectivo para la app A si se cumple alguna de estas tres:
/// <list type="number">
/// <item>Es de A — <c>AppId == A</c>. Es el alcance histórico, el único que existía antes.</item>
/// <item>Es de un <b>ancestro</b> de A y es público. La herencia se resuelve contra
/// <see cref="AppAncestor"/>.</item>
/// <item>Tiene un <b>grant explícito</b> a A (<see cref="Roles.RoleApp"/> /
/// <see cref="Permissions.PermissionApp"/>). Es la vía de lo privado: se comparte con una app hija
/// puntual sin marcarlo público y que lo hereden todas.</item>
/// </list>
/// <para>
/// El alcance <b>nunca</b> cruza tenants. No hace falta filtrar por tenant acá: el árbol de apps
/// es intra-tenant por construcción (las tres vías de creación de app usan el tenant del creador,
/// y re-parentar exige que el padre sea del mismo tenant), así que la regla 2 no puede traer nada
/// de afuera. Los callers igual mantienen su propio filtro por tenant, que es el candado duro.
/// </para>
/// <para>
/// <b>Si cambiás esta regla</b>, hay dos lugares que la repiten con otra forma sintáctica porque
/// no son queries raíz y no pueden usar estas extensiones: el filtro de roles de la sesión en
/// <c>SessionRepository.GetByTokenAsync</c> y el <c>Include</c> filtrado del login en
/// <c>AccountRepository.GetByIdAsync(id, appId)</c>. Los dos tienen un comentario que apunta acá.
/// </para>
/// </remarks>
internal static class EffectiveScope
{
    /// <summary>Roles efectivos para una app.</summary>
    internal static IQueryable<RoleEfCore> EffectiveForApp(
        this IQueryable<RoleEfCore> roles,
        AuthDbContext context,
        Guid appId)
    {
        return roles
            .Where(r =>
                r.AppId == appId
                || (r.IsPublic && context.AppsAncestors.Any(aa =>
                    aa.AppId == appId &&
                    aa.AncestorId == r.AppId))
                || context.RolesApps.Any(ra =>
                    ra.RoleId == r.Id &&
                    ra.AppId == appId));
    }

    /// <summary>
    /// Roles efectivos para <b>alguna</b> de las apps indicadas. Para los casos que razonan sobre
    /// todas las apps de una cuenta y no sobre la app logueada.
    /// </summary>
    internal static IQueryable<RoleEfCore> EffectiveForAnyApp(
        this IQueryable<RoleEfCore> roles,
        AuthDbContext context,
        List<Guid> appIds)
    {
        return roles
            .Where(r =>
                appIds.Contains(r.AppId)
                || (r.IsPublic && context.AppsAncestors.Any(aa =>
                    appIds.Contains(aa.AppId) &&
                    aa.AncestorId == r.AppId))
                || context.RolesApps.Any(ra =>
                    ra.RoleId == r.Id &&
                    appIds.Contains(ra.AppId)));
    }

    /// <summary>Permisos efectivos para una app.</summary>
    internal static IQueryable<PermissionEfCore> EffectiveForApp(
        this IQueryable<PermissionEfCore> permissions,
        AuthDbContext context,
        Guid appId)
    {
        return permissions
            .Where(p =>
                p.AppId == appId
                || (p.IsPublic && context.AppsAncestors.Any(aa =>
                    aa.AppId == appId &&
                    aa.AncestorId == p.AppId))
                || context.PermissionsApps.Any(pa =>
                    pa.PermissionId == p.Id &&
                    pa.AppId == appId));
    }
}
