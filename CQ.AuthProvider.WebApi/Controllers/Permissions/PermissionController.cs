using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using CQ.AuthProvider.WebApi.Extensions;
using CQ.ApiElements.Filters.Authorizations;
using CQ.AuthProvider.BusinessLogic.Permissions;
using CQ.UnitOfWork.Abstractions.Repositories;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.ApiElements.Filters.Authentications;

namespace CQ.AuthProvider.WebApi.Controllers.Permissions;

[ApiController]
[Route("permissions")]
[BearerAuthentication]
[SecureAuthorization]
public class PermissionController(
    [FromKeyedServices(MapperKeyedService.Presentation)] IMapper mapper,
    IPermissionService permissionService)
    : ControllerBase
{
    [HttpPost]
    public async Task CreateAsync(CreatePermissionArgs request)
    {
        var accountLogged = this.GetAccountLogged();

        await permissionService
            .CreateAsync(
            request,
            accountLogged)
            .ConfigureAwait(false);
    }

    [HttpPost("bulk")]
    public async Task CreateBulkAsync(CreateBulkPermissionArgs request)
    {
        var accountLogged = this.GetAccountLogged();

        await permissionService
            .CreateBulkAsync(
            request,
            accountLogged)
            .ConfigureAwait(false);
    }

    [HttpPut("bulk")]
    public async Task UpdateBulkAsync(UpdateBulkPermissionArgs request)
    {
        var accountLogged = this.GetAccountLogged();

        await permissionService
            .UpdateBulkAsync(
            request,
            accountLogged)
            .ConfigureAwait(false);
    }

    [HttpPut("{id:guid}")]
    public async Task UpdateAsync(
        Guid id,
        UpdatePermissionArgs request)
    {
        var accountLogged = this.GetAccountLogged();

        await permissionService
            .UpdateAsync(
            id,
            request,
            accountLogged)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Da acceso explícito al permiso a apps descendientes de su app dueña. La vía para compartir
    /// un permiso privado con una app hija puntual, en vez de marcarlo público y que lo hereden
    /// todas.
    /// </summary>
    [HttpPost("{id:guid}/apps")]
    public async Task AddAppsAsync(Guid id, AddAppsArgs request)
    {
        var accountLogged = this.GetAccountLogged();

        await permissionService
            .AddAppsByIdAsync(
            id,
            request,
            accountLogged)
            .ConfigureAwait(false);
    }

    [HttpDelete("{id:guid}/apps/{appId:guid}")]
    public async Task RemoveAppAsync(Guid id, Guid appId)
    {
        var accountLogged = this.GetAccountLogged();

        await permissionService
            .RemoveAppByIdAsync(
            id,
            appId,
            accountLogged)
            .ConfigureAwait(false);
    }

    [HttpGet]
    public async Task<Pagination<PermissionBasicInfoResponse>> GetAllAsync(
        [FromQuery] Guid? appId,
        [FromQuery] bool? isPrivate,
        [FromQuery] Guid? roleId,
        [FromQuery] string? name,
        [FromQuery] string? key,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var accountLogged = this.GetAccountLogged();

        var permissions = await permissionService
            .GetAllAsync(
            appId,
            isPrivate,
            roleId,
            name,
            key,
            page,
            pageSize,
            accountLogged)
            .ConfigureAwait(false);

        return mapper.Map<Pagination<PermissionBasicInfoResponse>>(permissions);
    }
}
