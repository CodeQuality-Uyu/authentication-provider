namespace CQ.AuthProvider.BusinessLogic.GoogleAuth.Exceptions
{
    public sealed class GoogleSignInNotConfiguredException : Exception
    {
        public Guid AppId { get; }

        public GoogleSignInNotConfiguredException(Guid appId)
        {
            AppId = appId;
        }
    }
}
