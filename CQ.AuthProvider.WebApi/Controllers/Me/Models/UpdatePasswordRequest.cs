﻿using CQ.ApiElements.Dtos;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.Utility;

namespace CQ.AuthProvider.WebApi.Controllers.Me.Models;

public sealed record class UpdatePasswordRequest
    : Request<UpdatePasswordArgs>
{
    public string? Email { get; init; }

    public string? OldPassword { get; init; }

    public string? NewPassword { get; init; }

    protected override UpdatePasswordArgs InnerMap()
    {
        Guard.ThrowIsNullOrEmpty(Email, nameof(Email));
        Guard.ThrowIsNullOrEmpty(OldPassword, nameof(OldPassword));
        Guard.ThrowIsNullOrEmpty(NewPassword, nameof(NewPassword));

        return new UpdatePasswordArgs(
            Email!,
            OldPassword!,
            NewPassword!);
    }
}
