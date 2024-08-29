using CQ.AuthProvider.BusinessLogic.Abstractions.ResetPasswords;
using CQ.AuthProvider.WebApi.Controllers.ResetPasswords.Models;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc;

namespace CQ.AuthProvider.WebApi.Controllers.ResetPasswords;

[ApiController]
[Route("reset-passwords")]
public class ResetPasswordController(IResetPasswordService resetPasswordService)
    : ControllerBase
{
    [HttpPost]
    public async Task CreateAsync(ResetPasswordRequest? request)
    {
        Guard.ThrowIsNull(request, nameof(request));

        var email = request.Map();

        await resetPasswordService
            .CreateAsync(email)
            .ConfigureAwait(false);
    }

    [HttpPut("{id:Guid}")]
    public async Task AcceptAsync(
        string id,
        ResetPasswordAcceptedRequest? request)
    {
        Guard.ThrowIsNull(request, nameof(request));

        var args = request.Map();

        await resetPasswordService
            .AcceptAsync(
            id,
            args)
            .ConfigureAwait(false);
    }
}
