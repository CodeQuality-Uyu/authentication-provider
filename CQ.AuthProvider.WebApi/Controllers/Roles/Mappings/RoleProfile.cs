﻿using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Abstractions.Roles;
using CQ.AuthProvider.WebApi.Controllers.Roles.Models;

namespace CQ.AuthProvider.WebApi.Controllers.Roles.Mappings;

internal sealed class RoleProfile
    : Profile
{
    public RoleProfile()
    {
        #region Get all
        CreateMap<Role, RoleBasicInfoResponse>();
        CreateMap<RoleKey, string>()
            .ConvertUsing((source, destination, context) => source.ToString());
        #endregion
    }
}