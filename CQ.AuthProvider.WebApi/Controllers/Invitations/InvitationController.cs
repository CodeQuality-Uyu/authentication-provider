using AutoMapper;
using CQ.ApiElements.Filters.Authorizations;
using CQ.AuthProvider.BusinessLogic.Abstractions.Accounts;
using CQ.AuthProvider.BusinessLogic.Abstractions.Invitations;
using CQ.AuthProvider.WebApi.Controllers.Accounts.Models;
using CQ.AuthProvider.WebApi.Controllers.Invitations.Models;
using CQ.AuthProvider.WebApi.Extensions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc;

namespace CQ.AuthProvider.WebApi.Controllers.Invitations;

[ApiController]
[Route("invitations")]
public sealed class InvitationController(
    IMapper mapper,
    IInvitationService invitationService)
    : ControllerBase
{
    [HttpPost]
    [SecureAuthorization]
    public async Task CreateAsync(CreateInvitationRequest? request)
    {
        Guard.ThrowIsNull(request, nameof(request));

        var args = request.Map();

        var accountLogged = this.GetAccountLogged();

        await invitationService
            .CreateAsync(
            args,
            accountLogged)
            .ConfigureAwait(false);
    }

    [HttpGet]
    [SecureAuthorization]
    public async Task<List<InvitationBasicInfoResponse>> GetAllAsync(
        [FromQuery] string? creatorId,
        [FromQuery] string? appId,
        [FromQuery] string? tenantId)
    {
        var accountLogged = this.GetAccountLogged();

        var invitations = await invitationService
            .GetAllAsync(
            creatorId,
            appId,
            tenantId,
            accountLogged)
            .ConfigureAwait(false);

        return mapper.Map<List<InvitationBasicInfoResponse>>(invitations);
    }

    [HttpPut("{id}/accept")]
    public async Task<AccountCreatedResponse> AcceptAsync(
        string id,
        AcceptInvitationRequest request)
    {
        var args = request.Map();

        var account = await invitationService
        .AcceptByIdAsync(
        id,
            args)
            .ConfigureAwait(false);


        return mapper.Map<AccountCreatedResponse>(account);
    }

    [HttpPut("{id}/declain")]
    public async Task DeclainAsync(
        string id,
        DeclainInvitationRequest request)
    {
        var args = request.Map();

        await invitationService
            .DeclainByIdAsync(
            id,
            args)
            .ConfigureAwait(false);
    }
}
