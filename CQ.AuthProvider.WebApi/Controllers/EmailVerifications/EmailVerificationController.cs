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

    [HttpPut("{id}")]
    public async Task AcceptAsync(
        Guid id,
        AcceptEmailVerificationArgs request)
    {
        await emailVerificationService
            .AcceptAsync(
            id,
            request)
            .ConfigureAwait(false);
    }
}
