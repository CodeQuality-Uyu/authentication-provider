﻿namespace CQ.AuthProvider.BusinessLogic.Accounts;

public sealed record CreateAccountArgs(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Locale,
    string TimeZone,
    string? ProfilePictureId,
    Guid AppId);

public sealed record CreateAccountForArgs(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Locale,
    string TimeZone,
    string? ProfilePictureId,
    Guid? AppId,
    Guid? RoleId);
