﻿
namespace CQ.AuthProvider.BusinessLogic.Invitations;

public sealed record AcceptInvitationArgs(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Locale,
    string TimeZone,
    Guid? ProfilePictureId,
    int Code);
