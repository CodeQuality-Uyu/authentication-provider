namespace CQ.AuthProvider.BusinessLogic.Apps;

/// <summary>
/// Reemplaza los logos de una app. Cada key es la de un temporal recién subido
/// (<c>POST /blobs</c>). Una key vacía o igual a la actual deja ese logo como está: no
/// es un reemplazo total, y no hay forma de quitar un logo (la app tiene los tres
/// siempre).
/// </summary>
public sealed record UpdateAppLogoArgs(
    string? ColorKey,
    string? LightKey,
    string? DarkKey);
