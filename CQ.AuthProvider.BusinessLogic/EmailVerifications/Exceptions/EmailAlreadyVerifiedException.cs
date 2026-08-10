namespace CQ.AuthProvider.BusinessLogic.EmailVerifications.Exceptions
{
    public sealed class EmailAlreadyVerifiedException : Exception
    {
        public string Email { get; }

        public EmailAlreadyVerifiedException(string email)
        {
            Email = email;
        }
    }
}
