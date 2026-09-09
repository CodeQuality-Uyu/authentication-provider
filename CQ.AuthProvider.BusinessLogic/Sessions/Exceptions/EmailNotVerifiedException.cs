namespace CQ.AuthProvider.BusinessLogic.Sessions.Exceptions
{
    public sealed class EmailNotVerifiedException : Exception
    {
        public string Email { get; }

        // True cuando, al detectar que la cuenta no está verificada, el código/token anterior
        // ya había vencido y se generó y mandó uno nuevo como parte de esta misma operación.
        // False cuando el que ya estaba pendiente sigue vigente (no se reenvió nada).
        public bool VerificationResent { get; }

        public EmailNotVerifiedException(
            string email,
            bool verificationResent = false)
        {
            Email = email;
            VerificationResent = verificationResent;
        }
    }
}
