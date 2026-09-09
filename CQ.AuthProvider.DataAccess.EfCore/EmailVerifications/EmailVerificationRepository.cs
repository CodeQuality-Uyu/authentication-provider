using AutoMapper;
using CQ.AuthProvider.BusinessLogic.EmailVerifications;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.UnitOfWork.EfCore.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.AuthProvider.DataAccess.EfCore.EmailVerifications;

internal sealed class EmailVerificationRepository(
    AuthDbContext _context,
    [FromKeyedServices(MapperKeyedService.DataAccess)] IMapper _mapper)
    : EfCoreRepository<EmailVerificationEfCore>(_context),
    IEmailVerificationRepository
{
    public async Task<EmailVerification> GetActiveForAcceptanceAsync(
        string email,
        string? token,
        int? code)
    {
        var emailVerificationEfCore = await FindActiveAsync(email, token, code)
            .ConfigureAwait(false);

        // AssertNullEntity solo usa este valor para el mensaje de error, no para buscar.
        AssertNullEntity(emailVerificationEfCore, email, nameof(EmailVerification.Email));

        return _mapper.Map<EmailVerification>(emailVerificationEfCore);
    }

    public async Task<EmailVerification> GetVerifiedForConsumptionAsync(
        string email,
        string? token,
        int? code)
    {
        var emailVerificationEfCore = await FindActiveAsync(email, token, code)
            .ConfigureAwait(false);

        if (emailVerificationEfCore is null || !emailVerificationEfCore.IsVerified)
        {
            AssertNullEntity(null, email, nameof(EmailVerification.Email));
        }

        return _mapper.Map<EmailVerification>(emailVerificationEfCore);
    }

    private async Task<EmailVerificationEfCore?> FindActiveAsync(
        string email,
        string? token,
        int? code)
    {
        var query = Entities
            .Where(e => e.Email == email)
            .Where(e => DateTime.UtcNow <= e.ExpiresAt);

        query = string.IsNullOrEmpty(token)
            ? query.Where(e => e.Code == code)
            : query.Where(e => e.Token == token);

        return await query
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<EmailVerification?> GetOrDefaultByEmailAsync(string email)
    {
        var emailVerificationEfCore = await Entities
            .Where(e => e.Email == email)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        return _mapper.Map<EmailVerification>(emailVerificationEfCore);
    }

    async Task IEmailVerificationRepository.CreateAndSaveAsync(EmailVerification emailVerification)
    {
        var emailVerificationEfCore = new EmailVerificationEfCore(
            emailVerification.Id,
            emailVerification.Email,
            emailVerification.Token,
            emailVerification.Code);

        await CreateAndSaveAsync(emailVerificationEfCore).ConfigureAwait(false);
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        await DeleteAndSaveAsync(e => e.Id == id)
            .ConfigureAwait(false);
    }

    public async Task UpdateByIdAsync(
        Guid id,
        string token,
        int code)
    {
        var emailVerification = await base.GetByIdAsync(id).ConfigureAwait(false);

        emailVerification.Token = token;
        emailVerification.Code = code;
        // Bug: un reenvío sobre un código ya vencido heredaba la ExpiresAt vieja (ya pasada),
        // así que el código "nuevo" nacía vencido. Se renueva la ventana completa acá.
        emailVerification.ExpiresAt = DateTime.UtcNow.AddMinutes(EmailVerification.TOLERANCE_IN_MINUTES);
        // Un reenvío invalida cualquier verificación previa sobre el código/token viejo.
        emailVerification.IsVerified = false;

        await UpdateAndSaveAsync(emailVerification).ConfigureAwait(false);
    }

    public async Task MarkAsVerifiedByIdAsync(Guid id)
    {
        var emailVerification = await base.GetByIdAsync(id).ConfigureAwait(false);

        emailVerification.IsVerified = true;

        await UpdateAndSaveAsync(emailVerification).ConfigureAwait(false);
    }
}
