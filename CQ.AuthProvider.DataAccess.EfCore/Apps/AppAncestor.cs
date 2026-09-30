namespace CQ.AuthProvider.DataAccess.EfCore.Apps;

/// <summary>
/// Cierre transitivo del árbol de apps: una fila por cada par (app, ancestro).
/// </summary>
/// <remarks>
/// Existe para que resolver "los roles y permisos públicos que una app hereda de sus ancestros"
/// sea un join plano dentro de la misma query, en vez de subir la cadena de
/// <see cref="AppEfCore.FatherAppId"/> con un query por nivel. El camino de sesión lo consulta en
/// <b>cada request autenticado</b>: de ahí la desnormalización.
/// <para>
/// No es la fuente de verdad del árbol — esa sigue siendo <see cref="AppEfCore.FatherAppId"/>.
/// Es un índice derivado, y se reconstruye en <c>AppRepository</c> al crear una app y al
/// re-parentarla. El chequeo de ciclos al re-parentar lee la columna, no esta tabla, justamente
/// para que un drift acá no habilite un ciclo.
/// </para>
/// </remarks>
public sealed record class AppAncestor()
{
    public required Guid AppId { get; init; }

    public AppEfCore App { get; init; } = null!;

    public required Guid AncestorId { get; init; }

    public AppEfCore Ancestor { get; init; } = null!;

    /// <summary>1 = padre directo, 2 = abuelo, y así hacia arriba.</summary>
    public required int Depth { get; init; }
}
