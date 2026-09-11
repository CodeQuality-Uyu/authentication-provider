namespace CQ.AuthProvider.BusinessLogic.GoogleAuth.Exceptions
{
    public sealed class AccountNotInAppException : Exception
    {
        public string Email { get; }

        public Guid AppId { get; }

        public AccountNotInAppException(string email, Guid appId)
        {
            Email = email;
            AppId = appId;
        }
    }
}
