namespace CQ.AuthProvider.BusinessLogic.GoogleAuth;

/// <summary>
/// Links an <see cref="Accounts.Account"/> to a Google account (by its stable
/// <c>sub</c> claim). Lives in the Identity database, separate from account
/// data, the same way password-based <see cref="Identities.Identity"/> does.
/// An account without a row here simply has no Google login linked yet; its
/// password login (if any) keeps working untouched.
/// </summary>
public sealed record class GoogleIdentity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required Guid AccountId { get; init; }

    public required string GoogleSub { get; init; }
}
