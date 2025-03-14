﻿namespace CQ.AuthProvider.BusinessLogic.Accounts;

public sealed record CreateAccountArgs(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Locale,
    string TimeZone,
    Guid? ProfilePictureId,
    Guid AppId);

public sealed record CreateAccountForArgs(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Locale,
    string TimeZone,
    Guid? ProfilePictureId,
    List<Guid>? AppIds,
    List<Guid>? RoleIds);
