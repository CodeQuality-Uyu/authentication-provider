﻿using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Utils;

namespace CQ.AuthProvider.WebApi.Controllers.Accounts;

internal sealed class AccountProfile
    : Profile
{
    public AccountProfile()
    {
        this.CreatePaginationMap<Account, AccountBasicInfoResponse>();
    }
}