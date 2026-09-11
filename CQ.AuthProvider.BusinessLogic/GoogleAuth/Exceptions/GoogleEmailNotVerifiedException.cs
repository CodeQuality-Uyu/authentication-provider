namespace CQ.AuthProvider.BusinessLogic.GoogleAuth.Exceptions
{
    public sealed class GoogleEmailNotVerifiedException : Exception
    {
        public string Email { get; }

        public GoogleEmailNotVerifiedException(string email)
        {
            Email = email;
        }
    }
}
