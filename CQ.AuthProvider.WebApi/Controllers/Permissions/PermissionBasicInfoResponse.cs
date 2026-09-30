namespace CQ.AuthProvider.WebApi.Controllers.Permissions;

public sealed record PermissionBasicInfoResponse(
    Guid Id,
    string Name,
    string Description,
    string Key,
    bool IsPublic,
    PermissionAppBasicInfoResponse App);

/// <summary>
/// App dueña del permiso. Sin ella un cliente no puede editarlo: el PUT pide el appId y lo
/// guarda, así que mandar otro lo muda de app.
/// </summary>
public sealed record PermissionAppBasicInfoResponse(
    Guid Id,
    string Name);
