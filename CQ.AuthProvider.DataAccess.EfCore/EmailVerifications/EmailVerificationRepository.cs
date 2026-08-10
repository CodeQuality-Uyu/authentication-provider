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
        Guid id,
        string email,
        string? token,
        int? code)
    {
        var query = Entities
            .Where(e => e.Id == id)
            .Where(e => e.Account.Email == email)
            .Where(e => DateTime.UtcNow <= e.ExpiresAt);

        query = string.IsNullOrEmpty(token)
            ? query.Where(e => e.Code == code)
            : query.Where(e => e.Token == token);

        var emailVerification = await query
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        AssertNullEntity(emailVerification, id, nameof(EmailVerification.Id));

        return _mapper.Map<EmailVerification>(emailVerification);
    }

    public async Task<EmailVerification?> GetOrDefaultByEmailAsync(string email)
    {
        var query = Entities
            .Include(e => e.Account)
            .Where(e => e.Account.Email == email);

        var emailVerification = await query
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        return _mapper.Map<EmailVerification>(emailVerification);
    }

    async Task IEmailVerificationRepository.CreateAndSaveAsync(EmailVerification emailVerification)
    {
        var emailVerificationEfCore = new EmailVerificationEfCore(
            emailVerification.Id,
            emailVerification.Token,
            emailVerification.Code,
            emailVerification.Account.Id);

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

        await UpdateAndSaveAsync(emailVerification).ConfigureAwait(false);
    }
}
