namespace CQ.AuthProvider.BusinessLogic.GoogleAuth.Exceptions
{
    public sealed class InvalidGoogleTokenException : Exception
    {
        public InvalidGoogleTokenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
