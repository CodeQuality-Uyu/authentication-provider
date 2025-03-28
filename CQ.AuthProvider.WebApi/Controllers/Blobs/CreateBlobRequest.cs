﻿namespace CQ.AuthProvider.WebApi.Controllers.Blobs;

public sealed record CreateBlobRequest(
    Guid? AppId,
    string? Key,
    string ContentType);
