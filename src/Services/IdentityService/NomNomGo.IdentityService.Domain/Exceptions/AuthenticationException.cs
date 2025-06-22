namespace NomNomGo.IdentityService.Domain.Exceptions
{
    public class AuthenticationException : ApplicationException
    {
        public AuthenticationException()
            : base("Ошибка аутентификации.")
        {
        }

        public AuthenticationException(string message)
            : base(message)
        {
        }
    }
}