﻿using AutoMapper;
using CQ.ApiElements.Filters.Authentications;
using CQ.ApiElements.Filters.Authorizations;
using CQ.AuthProvider.BusinessLogic.Invitations;
using CQ.AuthProvider.WebApi.Controllers.Accounts.Models;
using CQ.AuthProvider.WebApi.Controllers.Invitations.Models;
using CQ.AuthProvider.WebApi.Extensions;
using CQ.UnitOfWork.Abstractions.Repositories;
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
    [SecureAuthentication]
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
    [SecureAuthentication]
    [SecureAuthorization]
    public async Task<Pagination<InvitationBasicInfoResponse>> GetAllAsync(
        [FromQuery] string? creatorId,
        [FromQuery] string? appId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var accountLogged = this.GetAccountLogged();

        var invitations = await invitationService
            .GetAllAsync(
            creatorId,
            appId,
            page,
            pageSize,
            accountLogged)
            .ConfigureAwait(false);

        return mapper.Map<Pagination<InvitationBasicInfoResponse>>(invitations);
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
