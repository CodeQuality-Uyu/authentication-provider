using CQ.AuthProvider.BusinessLogic.ResetPasswords;
using Microsoft.AspNetCore.Mvc;

namespace CQ.AuthProvider.WebApi.Controllers.ResetPasswords;

[ApiController]
[Route("reset-passwords")]
public class ResetPasswordController(IResetPasswordService resetPasswordService)
    : ControllerBase
{
    [HttpPost]
    public async Task CreateAsync(CreateResetPasswordArgs request)
    {
        await resetPasswordService
            .CreateAsync(request)
            .ConfigureAwait(false);
    }

    [HttpPost("verify")]
    public async Task VerifyAsync(VerifyResetPasswordArgs request)
    {
        await resetPasswordService
            .VerifyAsync(request)
            .ConfigureAwait(false);
    }

    [HttpPost("accept")]
    public async Task AcceptAsync(AcceptResetPasswordArgs request)
    {
        await resetPasswordService
            .AcceptAsync(request)
            .ConfigureAwait(false);
    }
}
