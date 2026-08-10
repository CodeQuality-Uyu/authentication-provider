namespace CQ.AuthProvider.BusinessLogic.Sessions.Exceptions
{
    public sealed class EmailNotVerifiedException : Exception
    {
        public string Email { get; }

        public EmailNotVerifiedException(string email)
        {
            Email = email;
        }
    }
}
