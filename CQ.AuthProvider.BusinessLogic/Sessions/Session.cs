﻿using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Apps;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.Sessions;

public sealed record class Session()
{
    public string Id { get; init; } = Db.NewId();

    public string Token { get; init; } = null!;

    public Account Account { get; init; } = null!;

    public App App { get; init; } = null!;

    public Session(
    Account account,
    App app,
    string token)
        : this()
    {
        Account = account;
        App = app;
        Token = token;
    }
}