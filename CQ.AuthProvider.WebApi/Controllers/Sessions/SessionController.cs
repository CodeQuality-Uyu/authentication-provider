using Microsoft.AspNetCore.Mvc;
using CQ.AuthProvider.WebApi.Extensions;
using CQ.AuthProvider.WebApi.Controllers.Sessions.Models;
using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Abstractions.Sessions;
using CQ.Utility;
using CQ.ApiElements.Filters.Authorizations;

namespace CQ.AuthProvider.WebApi.Controllers.Sessions;

[ApiController]
[Route("sessions")]
public class SessionController(
    IMapper mapper,
    ISessionService sessionService)
    : ControllerBase
{
    [HttpPost("credentials")]
    public async Task<SessionCreatedResponse> CreateAsync(CreateSessionCredentialsRequest? request)
    {
        Guard.ThrowIsNull(request, nameof(request));

        var createAuth = request.Map();

        var session = await sessionService
            .CreateAsync(createAuth)
            .ConfigureAwait(false);

        return mapper.Map<SessionCreatedResponse>(session);
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
