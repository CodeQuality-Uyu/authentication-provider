﻿namespace CQ.AuthProvider.BusinessLogic.Accounts;

public sealed record class CreateAccountResult(
    string Id,
    string Email,
    string FullName,
    string FirstName,
    string LastName,
    string? ProfilePictureId,
    string Locale,
    string TimeZone,
    string Token,
    List<string> Roles,
    List<string> Permissions);
