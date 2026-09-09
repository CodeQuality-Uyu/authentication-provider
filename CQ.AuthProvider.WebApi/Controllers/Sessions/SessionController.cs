using Microsoft.AspNetCore.Mvc;
using CQ.AuthProvider.WebApi.Extensions;
using AutoMapper;
using CQ.ApiElements.Filters.Authorizations;
using CQ.AuthProvider.BusinessLogic.GoogleAuth;
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
    ISessionService sessionService,
    IGoogleAuthService googleAuthService)
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

    // Logs in (auto-provisioning the account on first login) using a Google
    // Identity Services id_token instead of email/password. Doesn't touch or
    // replace credentials login above.
    [HttpPost("google")]
    public async Task<SessionCreatedResponse> CreateWithGoogleAsync(CreateSessionGoogleArgs request)
    {
        var session = await googleAuthService
            .CreateSessionAsync(request)
            .ConfigureAwait(false);

        return mapper.Map<SessionCreatedResponse>(session);
    }

    [BearerAuthentication]
    [SecureAuthorization]
    [HttpPost("check")]
    public IActionResult Check(CheckRequest request)
    {
        var accountLogged = this.GetAccountLogged();

        if (Guard.IsNullOrEmpty(request.Permission))
        {
            return Ok(accountLogged);
        }

        var hasPermission = accountLogged.IsInRole(request.Permission);
        if (!hasPermission)
        {
            return new ObjectResult(new
            {
                code = "Forbidden",
                message = "Insufficient permissions",
                description = $"You don't have the permission {request.Permission} to access this request"
            })
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }
        return Ok(accountLogged);
    }

    [HttpDelete]
    [SecureAuthorization]
    public async Task DeleteAsync()
    {
        var accountLogged = this.GetAccountLogged();

        await sessionService
            .DeleteAsync(accountLogged)
            .ConfigureAwait(false);
    }
}
