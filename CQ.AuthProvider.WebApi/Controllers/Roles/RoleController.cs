using Microsoft.AspNetCore.Mvc;
using CQ.AuthProvider.WebApi.Filters;
using CQ.AuthProvider.WebApi.Extensions;
using CQ.AuthProvider.WebApi.Controllers.Roles.Models;
using AutoMapper;
using CQ.Utility;
using CQ.AuthProvider.BusinessLogic.Abstractions.Roles;
using CQ.AuthProvider.WebApi.Controllers.Permissions.Models;
using CQ.ApiElements.Filters.Authorizations;

namespace CQ.AuthProvider.WebApi.Controllers.Roles;

[ApiController]
[Route("roles")]
[CQAuthentication]
[SecureAuthorization]
public class RoleController(
    IMapper mapper,
    IRoleService roleService) : ControllerBase
{
    [HttpPost]
    public async Task CreateAsync(CreateRoleRequest? request)
    {
        Guard.ThrowIsNull(request, nameof(request));

        var args = request.Map();

        var accountLogged = this.GetAccountLogged();

        await roleService
            .CreateAsync(
            args,
            accountLogged)
            .ConfigureAwait(false);
    }

    [HttpPost("bulk")]
    public async Task CreateBulkAsync(CreateRoleBulkRequest? request)
    {
        Guard.ThrowIsNull(request, nameof(request));

        var args = request.Map();

        var accountLogged = this.GetAccountLogged();

        await roleService
            .CreateBulkAsync(
            args,
            accountLogged)
            .ConfigureAwait(false);
    }

    [HttpPost("{id}/permissions")]
    public async Task AddPermissionAsync(string id, AddPermissionRequest request)
    {
        Db.ThrowIsInvalidId(id, nameof(id));
        Guard.ThrowIsNull(request, nameof(request));

        var args = request.Map();

        await roleService
            .AddPermissionByIdAsync(
            id,
            args)
            .ConfigureAwait(false);
    }

    [HttpGet]
    public async Task<List<RoleBasicInfoResponse>> GetAllAsync(
        [FromQuery] string? appId,
        [FromQuery] bool? isPrivate,
        [FromQuery] string? tenantId)
    {
        var accountLogged = this.GetAccountLogged();

        var roles = await roleService
            .GetAllAsync(
            appId,
            isPrivate,
            tenantId,
            accountLogged)
            .ConfigureAwait(false);

        return mapper.Map<List<RoleBasicInfoResponse>>(roles);
    }
}
