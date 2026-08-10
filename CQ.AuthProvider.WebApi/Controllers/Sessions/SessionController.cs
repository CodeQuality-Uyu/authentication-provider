using Microsoft.AspNetCore.Mvc;
using CQ.AuthProvider.WebApi.Extensions;
using AutoMapper;
using CQ.ApiElements.Filters.Authorizations;
using CQ.AuthProvider.BusinessLogic.Sessions;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.ApiElements.Filters.Authentications;
using CQ.Utility;
using System.Net;

namespace CQ.AuthProvider.WebApi.Controllers.Sessions;

[ApiController]
[Route("sessions")]
public class SessionController(
    [FromKeyedServices(MapperKeyedService.Presentation)] IMapper mapper,
    ISessionService sessionService)
    : ControllerBase
{
    [HttpPost("credentials")]
    public async Task<SessionCreatedResponse> CreateAsync(CreateSessionCredentialsArgs request)
    {
        var session = await sessionService
            .CreateAsync(request)
            .ConfigureAwait(false);

        return mapper.Map<SessionCreatedResponse>(session);
    }

    /// <summary>
    /// Exchanges a refresh token for a new access token. The refresh token is
    /// rotated, so the one sent stops working once this call succeeds.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<SessionCreatedResponse> RefreshAsync(RefreshSessionArgs request)
    {
        var session = await sessionService
            .RefreshAsync(request)
            .ConfigureAwait(false);

        return mapper.Map<SessionCreatedResponse>(session);
    }

    /// <summary>
    /// Revokes the refresh token of the current session. The access token
    /// already in hand keeps working until it expires.
    /// </summary>
    [HttpDelete]
    [BearerAuthentication]
    [SecureAuthorization]
    public async Task DeleteAsync()
    {
        var accountLogged = this.GetAccountLogged();

        await sessionService
            .DeleteAsync(accountLogged)
            .ConfigureAwait(false);
    }
}
