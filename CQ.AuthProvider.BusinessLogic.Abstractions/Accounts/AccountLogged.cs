﻿
using CQ.AuthProvider.BusinessLogic.Abstractions.Apps;

namespace CQ.AuthProvider.BusinessLogic.Abstractions.Accounts;

public record class AccountLogged()
    : Account
{
    public string Token { get; init; } = null!;

    public App AppLogged { get; init; } = null!;

    public AccountLogged(
        Account account,
        string token,
        App appLogged)
        : this()
    {
        Id = account.Id;
        Email = account.Email;
        FirstName = account.FirstName;
        LastName = account.LastName;
        FullName = account.FullName;
        ProfilePictureUrl = account.ProfilePictureUrl;
        Locale = account.Locale;
        TimeZone = account.TimeZone;
        Roles = account.Roles;
        Tenant = account.Tenant;
        Token = token;
        AppLogged = appLogged;
    }
}