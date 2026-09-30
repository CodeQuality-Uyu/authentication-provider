using AutoMapper;
using CQ.ApiElements.Filters.Authentications;
using CQ.ApiElements.Filters.Authorizations;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.AuthProvider.WebApi.Extensions;
using CQ.UnitOfWork.Abstractions.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CQ.AuthProvider.WebApi.Controllers.Apps;

[ApiController]
[Route("apps")]
public sealed class AppController(
    IAppService appService,
    [FromKeyedServices(MapperKeyedService.Presentation)] IMapper _mapper)
    : ControllerBase
{
    [HttpPost]
    [BearerAuthentication]
    [SecureAuthorization]
    public async Task<AppCreatedResponse> CreateAsync(CreateAppArgs request)
    {
        var accountLogged = this.GetAccountLogged();

        var appCreated = await appService
            .CreateAsync(request, accountLogged)
            .ConfigureAwait(false);

        return _mapper.Map<AppCreatedResponse>(appCreated);
    }

    [HttpPost("client")]
    [SecureAuthentication(null, "Bearer", "Subscription")]
    [SecureAuthorization]
    public async Task<AppCreatedResponse> CreateClientAsync(CreateClientAppArgs request)
    {
        var accountLogged = this.GetAccountLogged();

        var appCreated = await appService
            .CreateClientAsync(request, accountLogged)
            .ConfigureAwait(false);

        return _mapper.Map<AppCreatedResponse>(appCreated);
    }

    [HttpGet]
    [BearerAuthentication()]
    [SecureAuthorization("getall-app", "getall-client")]
    public async Task<Pagination<AppBasicInfoResponse>> GetAllAsync(
        [FromQuery] Guid? fatherAppId = null,
        [FromQuery] Guid? tenantId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var accountLogged = this.GetAccountLogged();

        var apps = await appService
            .GetPaginationAsync(
            tenantId,
            fatherAppId,
            page,
            pageSize,
            accountLogged)
            .ConfigureAwait(false);

        return _mapper.Map<Pagination<AppBasicInfoResponse>>(apps);
    }

    [HttpGet("{id}")]
    public async Task<AppDetailInfoResponse> GetByIdAsync(Guid id)
    {
        var app = await appService
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        return _mapper.Map<AppDetailInfoResponse>(app);
    }

    [HttpPatch("{id}/colors")]
    [BearerAuthentication]
    [SecureAuthorization]
    public async Task UpdateColorsAsync(
        Guid id,
        Background request)
    {
        var accountLogged = this.GetAccountLogged();

        await appService
            .UpdateColorsByIdAsync(
            id,
            request,
            accountLogged)
            .ConfigureAwait(false);
    }

    [HttpPut("{id:guid}")]
    [BearerAuthentication]
    [SecureAuthorization]
    public async Task UpdateAsync(
        Guid id,
        UpdateAppArgs request)
    {
        var accountLogged = this.GetAccountLogged();

        await appService
            .UpdateByIdAsync(
            id,
            request,
            accountLogged)
            .ConfigureAwait(false);
    }

    [HttpPatch("{id:guid}/father")]
    [BearerAuthentication]
    [SecureAuthorization]
    public async Task UpdateFatherAsync(
        Guid id,
        UpdateAppFatherArgs request)
    {
        var accountLogged = this.GetAccountLogged();

        await appService
            .UpdateFatherByIdAsync(
            id,
            request,
            accountLogged)
            .ConfigureAwait(false);
    }
}
