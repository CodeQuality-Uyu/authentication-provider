﻿using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.AuthProvider.BusinessLogic.Tenants;

namespace CQ.AuthProvider.BusinessLogic.Accounts;

public sealed record CreateAccountResult(
    Guid Id,
    string Email,
    string FullName,
    string FirstName,
    string LastName,
    Guid? ProfilePictureId,
    string Locale,
    string TimeZone,
    App AppLogged,
    string Token,
    List<string> Roles,
    List<string> Permissions,
    Tenant Tenant);
