namespace Domain.Exceptions;

public abstract class InvalidEntityException(string message) : Exception(message)
{
}