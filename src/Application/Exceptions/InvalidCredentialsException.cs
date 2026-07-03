namespace Application.Exceptions;

public class InvalidCredentialsException(string message = "Usuário ou senha inválidos.") : Exception(message)
{
}
