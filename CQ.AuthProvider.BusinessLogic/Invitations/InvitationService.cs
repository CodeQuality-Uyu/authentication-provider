using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.AuthProvider.BusinessLogic.Emails;
using CQ.AuthProvider.BusinessLogic.Identities;
using CQ.AuthProvider.BusinessLogic.Roles;
using CQ.UnitOfWork.Abstractions;
using CQ.UnitOfWork.Abstractions.Repositories;
using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.Invitations;

internal sealed class InvitationService(
    IInvitationRepository _invitationRepository,
    IRoleRepository _roleRepository,
    IEmailService _emailService,
    IAccountEmailBrandingResolver _brandingResolver,
    IIdentityRepository _identityRepository,
    IAccountInternalService _accountService,
    IUnitOfWork _unitOfWork)
    : IInvitationService
{

    public async Task CreateAsync(
        CreateInvitationArgs args,
        AccountLogged accountLogged)
    {
        var app = accountLogged.Apps.Find(a => a.Id == args.AppId);
        if (args.AppId.HasValue && Guard.IsNull(app))
        {
            throw new InvalidOperationException($"App is not allowed");
        }
        app ??= accountLogged.AppLogged;

        var role = await _roleRepository
            .GetByIdAsync(args.RoleId)
            .ConfigureAwait(false);
        if (role.AppId != app.Id)
        {
            throw new InvalidOperationException($"Role not in app");
        }

        var existPendingInvitation = await _invitationRepository
            .ExistPendingByEmailAsync(args.Email)
            .ConfigureAwait(false);
        if (existPendingInvitation)
        {
            throw new InvalidOperationException($"Exist a pending invitation for the email");
        }

        await _accountService
            .AssertByEmailAsync(args.Email)
            .ConfigureAwait(false);

        var invitation = Invitation.New(
            args.Email,
            role,
            app,
            accountLogged);

        var logoUrl = await _brandingResolver
            .GetLogoUrlAsync(accountLogged.Tenant.Id)
            .ConfigureAwait(false);

        await _emailService
            .SendInviteUserAsync(
            args.Email,
            accountLogged.FullName,
            invitation.Code,
            logoUrl)
            .ConfigureAwait(false);

        await _invitationRepository
            .CreateAndSaveAsync(invitation)
            .ConfigureAwait(false);
    }

    public async Task<Pagination<Invitation>> GetAllAsync(
        Guid? creatorId,
        Guid? appId,
        int page,
        int pageSize,
        AccountLogged accountLogged)
    {
        var invitations = await _invitationRepository
            .GetAllAsync(
            creatorId,
            appId,
            page,
            pageSize,
            accountLogged)
            .ConfigureAwait(false);

        return invitations;
    }

    public async Task<CreateAccountResult> AcceptByIdAsync(
        Guid id,
        AcceptInvitationArgs args)
    {
        var invitation = await _invitationRepository
            .GetPendingByIdAsync(id)
            .ConfigureAwait(false);

        if (invitation.Code != args.Code ||
            invitation.Email != args.Email)
        {
            throw new InvalidOperationException("Invalid email or code");
        }

        await _invitationRepository
            .DeleteByIdAsync(id)
            .ConfigureAwait(false);

        var firstName = Guard.Normalize(args.FirstName);
        var lastName = Guard.Normalize(args.LastName);
        var account = Account.NewFromInvitation(
            args.Email,
            firstName,
            lastName,
            args.ProfilePictureKey,
            args.Locale,
            args.TimeZone,
            invitation);

        var result = await _accountService
                .CreateIdentityAndSaveAsync(account, args.Password)
                .ConfigureAwait(false);

        try
        {
            await _unitOfWork.CommitChangesAsync();
        }
        catch (Exception)
        {
            await _identityRepository
                .DeleteAndSaveByIdAsync(account.Id)
                .ConfigureAwait(false);
        }

        return result;
    }

    public async Task DeclainByIdAsync(
        Guid id,
        DeclainInvitationArgs args)
    {
        var invitation = await _invitationRepository
            .GetPendingByIdAsync(id)
            .ConfigureAwait(false);

        if (invitation.Email != args.Email && invitation.Code != args.Code)
        {
            throw new InvalidOperationException("The info of the invitation is invalid");
        }

        await _invitationRepository
            .DeleteAndSaveByIdAsync(id)
            .ConfigureAwait(false);
    }
}
