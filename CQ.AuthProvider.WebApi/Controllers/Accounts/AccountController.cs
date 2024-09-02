using Microsoft.AspNetCore.Mvc;
using CQ.AuthProvider.WebApi.Extensions;
using CQ.AuthProvider.WebApi.Controllers.Accounts.Models;
using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Abstractions.Accounts;
using CQ.ApiElements.Filters.Authorizations;

namespace CQ.AuthProvider.WebApi.Controllers.Accounts;

[ApiController]
[Route("accounts")]
public class AccountController(
    IAccountService accountService,
    IMapper mapper)
    : ControllerBase
{
    [HttpPost("credentials")]
    public async Task<AccountCreatedResponse> CreateCredentialsAsync(CreateAccountRequest request)
    {
        var createAccount = request.Map();

        var account = await accountService
            .CreateAsync(createAccount)
            .ConfigureAwait(false);

        return mapper.Map<AccountCreatedResponse>(account);
    }

    [HttpPost("credentials/for")]
    [SecureAuthorization]
    public async Task CreateCredentialsForAsync(CreateAccountRequest request)
    {
        var createAccountFor = request.Map();

        await accountService
            .CreateAsync(createAccountFor)
            .ConfigureAwait(false);
    }
}
