using CQ.AuthProvider.BusinessLogic.EmailVerifications;
using Microsoft.AspNetCore.Mvc;

namespace CQ.AuthProvider.WebApi.Controllers.EmailVerifications;

[ApiController]
[Route("email-verifications")]
public class EmailVerificationController(IEmailVerificationService emailVerificationService)
    : ControllerBase
{
    [HttpPost]
    public async Task CreateAsync(CreateEmailVerificationArgs request)
    {
        await emailVerificationService
            .CreateAsync(request)
            .ConfigureAwait(false);
    }

    [HttpPost("accept")]
    public async Task AcceptAsync(AcceptEmailVerificationArgs request)
    {
        await emailVerificationService
            .AcceptAsync(request)
            .ConfigureAwait(false);
    }
}
