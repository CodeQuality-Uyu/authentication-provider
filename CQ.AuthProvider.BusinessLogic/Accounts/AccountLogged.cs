﻿using CQ.AuthProvider.BusinessLogic.Apps;
using System.Security.Principal;

namespace CQ.AuthProvider.BusinessLogic.Accounts;

public record class AccountLogged()
    : Account,
    IPrincipal
{
    public string Token { get; init; } = null!;

    public List<Guid> AppsIds => Apps.ConvertAll(a => a.Id);

    public List<Guid> RolesIds => Roles.ConvertAll(r => r.Id);

    public List<string> PermissionsKeys => Roles.SelectMany(r => r.Permissions).Select(p => p.Key).ToList();

    public App AppLogged { get; init; } = null!;

    public virtual IIdentity? Identity => null;

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
        ProfilePictureId = account.ProfilePictureId;
        Locale = account.Locale;
        TimeZone = account.TimeZone;
        Roles = account.Roles;
        Tenant = account.Tenant;
        Token = token;
        AppLogged = appLogged;
        Apps = account.Apps;
    }

    public bool IsInRole(string role)
    {
        var isRole = Roles.Exists(r =>
        r.Name == role ||
        r.Key == role ||
        r.Id.ToString() == role);

        return isRole || HasPermission(role);
    }
}
